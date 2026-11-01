#!/usr/bin/env node
// Records the human side of an interaction on an existing log entry, then re-renders the summary.
//   node .claude/hooks/annotate.js last --decision "kept the lock, dismissed finding 8" --limitation "..." --verification "re-ran tests"
//   node .claude/hooks/annotate.js <id-or-prefix> --decision "..."
const fs = require('fs');
const path = require('path');
const { renderSummary } = require('./render-summary');

const root = process.env.CLAUDE_PROJECT_DIR || process.cwd();
const logFile = path.join(root, 'docs', 'ai-log', 'ai-interactions.jsonl');
const [target, ...rest] = process.argv.slice(2);

const options = {};
for (let index = 0; index < rest.length; index += 2) options[rest[index].replace(/^--/, '')] = rest[index + 1];

if (!target || !fs.existsSync(logFile)) {
  console.error('usage: annotate.js <last|id-prefix> [--decision t] [--limitation t] [--verification t] [--suggestion t]');
  process.exit(1);
}

const entries = fs.readFileSync(logFile, 'utf8').split('\n').filter(Boolean).map((line) => JSON.parse(line));
const entry = target === 'last' ? entries[entries.length - 1] : entries.find((candidate) => candidate.id.startsWith(target));
if (!entry) {
  console.error(`no entry matches "${target}"`);
  process.exit(1);
}

if (options.decision) entry.human_decision = options.decision;
if (options.limitation) entry.limitation = options.limitation;
if (options.suggestion) entry.suggestion = options.suggestion;
if (options.verification) entry.action_verification = [...(entry.action_verification || []), options.verification];

fs.writeFileSync(logFile, `${entries.map((item) => JSON.stringify(item)).join('\n')}\n`);
renderSummary(root);
console.log(`annotated ${entry.id}`);
