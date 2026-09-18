import * as yup from 'yup';

export const DOCUMENT_TYPES = [
  'Invoice',
  'Credit Note',
  'Estimate / Quote',
  'Recurring Invoice',
  'Delivery Challan',
];

export const RESET_POLICIES = [
  'Never',
  'Yearly',
  'Financial Year',
  'Monthly',
  'Daily',
];

export const SUPPORTED_TOKENS = [
  { token: '{YEAR}', label: 'Year (4-digit)', example: '2026', desc: 'Current 4-digit calendar year' },
  { token: '{YY}', label: 'Year (2-digit)', example: '26', desc: 'Current 2-digit calendar year' },
  { token: '{MONTH}', label: 'Month (Name)', example: 'Sep', desc: 'Abbreviated month name' },
  { token: '{MM}', label: 'Month (2-digit)', example: '09', desc: 'Zero-padded month number (01-12)' },
  { token: '{FY}', label: 'Financial Year', example: '26-27', desc: 'Indian Financial Year (Apr-Mar)' },
  { token: '{QUARTER}', label: 'Quarter', example: 'Q3', desc: 'Calendar quarter (Q1-Q4)' },
];

export const DEFAULT_PRESETS_BY_DOC_TYPE = {
  Invoice: {
    documentType: 'Invoice',
    prefix: 'INV-',
    suffix: '',
    tokens: '{YEAR}-',
    sequenceLength: 4,
    nextNumber: 42,
    resetPolicy: 'Financial Year',
  },
  'Credit Note': {
    documentType: 'Credit Note',
    prefix: 'CN-',
    suffix: '',
    tokens: '{YEAR}-',
    sequenceLength: 4,
    nextNumber: 1,
    resetPolicy: 'Financial Year',
  },
  'Estimate / Quote': {
    documentType: 'Estimate / Quote',
    prefix: 'EST-',
    suffix: '',
    tokens: '{YEAR}-',
    sequenceLength: 4,
    nextNumber: 1,
    resetPolicy: 'Yearly',
  },
  'Recurring Invoice': {
    documentType: 'Recurring Invoice',
    prefix: 'REC-',
    suffix: '',
    tokens: '{YEAR}-',
    sequenceLength: 4,
    nextNumber: 1,
    resetPolicy: 'Yearly',
  },
  'Delivery Challan': {
    documentType: 'Delivery Challan',
    prefix: 'DC-',
    suffix: '',
    tokens: '{YEAR}-',
    sequenceLength: 4,
    nextNumber: 1,
    resetPolicy: 'Financial Year',
  },
};

export const DEFAULT_NUMBERING_CONFIG = DEFAULT_PRESETS_BY_DOC_TYPE.Invoice;

/**
 * Evaluates date-based tokens within a template string.
 */
export const evaluateTokens = (text = '', date = new Date()) => {
  if (!text || typeof text !== 'string') return '';
  const d = date instanceof Date && !isNaN(date) ? date : new Date();
  const year = d.getFullYear();
  const yy = String(year).slice(-2);
  const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  const monthName = monthNames[d.getMonth()];
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  
  // Financial year calculation (Apr-Mar for India)
  const fyStart = d.getMonth() >= 3 ? year : year - 1;
  const fyEnd = fyStart + 1;
  const fy = `${String(fyStart).slice(-2)}-${String(fyEnd).slice(-2)}`;

  // Quarter calculation
  const quarter = `Q${Math.floor(d.getMonth() / 3) + 1}`;

  return text
    .replace(/\{YEAR\}/gi, String(year))
    .replace(/\{YY\}/gi, yy)
    .replace(/\{MONTH\}/gi, monthName)
    .replace(/\{MM\}/gi, mm)
    .replace(/\{FY\}/gi, fy)
    .replace(/\{QUARTER\}/gi, quarter);
};

/**
 * Formats a sequence number with leading zeros based on sequenceLength.
 */
export const formatSequence = (seq, length = 4) => {
  const num = parseInt(seq, 10);
  const validNum = isNaN(num) || num < 0 ? 1 : num;
  const len = Math.max(1, Math.min(12, parseInt(length, 10) || 4));
  return String(validNum).padStart(len, '0');
};

/**
 * Generates the live next document number preview and individual part breakdown.
 */
export const generateNumberPreview = (config = {}) => {
  const {
    prefix = '',
    suffix = '',
    tokens = '',
    sequenceLength = 4,
    nextNumber = 1,
    date = new Date(),
  } = config;

  const evaluatedPrefix = evaluateTokens(prefix, date);
  const evaluatedTokens = evaluateTokens(tokens, date);
  const paddedSequence = formatSequence(nextNumber, sequenceLength);
  const evaluatedSuffix = evaluateTokens(suffix, date);

  const fullPreview = `${evaluatedPrefix}${evaluatedTokens}${paddedSequence}${evaluatedSuffix}`;

  return {
    fullPreview,
    parts: {
      prefix: evaluatedPrefix,
      tokens: evaluatedTokens,
      sequence: paddedSequence,
      suffix: evaluatedSuffix,
    },
    raw: {
      prefix,
      tokens,
      sequence: nextNumber,
      sequenceLength,
      suffix,
    },
  };
};

// Regex validating prefix and suffix - allows letters, numbers, hyphens, slashes, underscores, and curly-brace tokens
const AFFIX_REGEX = /^[A-Za-z0-9_\-\/{}]*$/;

// Extract tokens from a string to validate them
const extractTokenNames = (str = '') => {
  const matches = str.match(/\{[^}]+\}/g) || [];
  return matches.map((m) => m.toUpperCase());
};

const VALID_TOKEN_STRINGS = SUPPORTED_TOKENS.map((t) => t.token.toUpperCase());

export const numberingValidationSchema = yup.object().shape({
  documentType: yup
    .string()
    .required('Document Type is required')
    .oneOf(DOCUMENT_TYPES, 'Select a valid Document Type'),

  prefix: yup
    .string()
    .trim()
    .max(20, 'Prefix must not exceed 20 characters')
    .matches(AFFIX_REGEX, 'Prefix can only contain letters, numbers, hyphens, slashes, underscores, and tokens')
    .test('valid-tokens-in-prefix', 'Prefix contains unsupported token. Supported: {YEAR}, {YY}, {MONTH}, {MM}, {FY}, {QUARTER}', (val) => {
      if (!val) return true;
      const tokens = extractTokenNames(val);
      return tokens.every((t) => VALID_TOKEN_STRINGS.includes(t));
    }),

  suffix: yup
    .string()
    .trim()
    .max(20, 'Suffix must not exceed 20 characters')
    .matches(AFFIX_REGEX, 'Suffix can only contain letters, numbers, hyphens, slashes, underscores, and tokens')
    .test('valid-tokens-in-suffix', 'Suffix contains unsupported token. Supported: {YEAR}, {YY}, {MONTH}, {MM}, {FY}, {QUARTER}', (val) => {
      if (!val) return true;
      const tokens = extractTokenNames(val);
      return tokens.every((t) => VALID_TOKEN_STRINGS.includes(t));
    }),

  tokens: yup
    .string()
    .trim()
    .max(30, 'Tokens expression must not exceed 30 characters')
    .test('valid-tokens-field', 'Tokens field contains unsupported token. Supported: {YEAR}, {YY}, {MONTH}, {MM}, {FY}, {QUARTER}', (val) => {
      if (!val) return true;
      const tokens = extractTokenNames(val);
      return tokens.every((t) => VALID_TOKEN_STRINGS.includes(t));
    }),

  sequenceLength: yup
    .number()
    .typeError('Sequence Length must be a number')
    .required('Sequence Length is required')
    .integer('Sequence Length must be a whole number')
    .min(3, 'Sequence Length must be at least 3 digits')
    .max(10, 'Sequence Length must not exceed 10 digits'),

  nextNumber: yup
    .number()
    .typeError('Next Number must be a valid number')
    .required('Starting / Next Number is required')
    .integer('Next Number must be a whole number')
    .min(1, 'Next Number must be at least 1'),

  resetPolicy: yup
    .string()
    .required('Reset Policy is required')
    .oneOf(RESET_POLICIES, 'Select a valid Reset Policy'),
});
