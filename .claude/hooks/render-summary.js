// Renders docs/ai-log/ai-interactions-summary.md from docs/ai-log/ai-interactions.jsonl.
const fs = require('fs');
const path = require('path');

function renderSummary(root) {
  const logFile = path.join(root, 'docs', 'ai-log', 'ai-interactions.jsonl');
  if (!fs.existsSync(logFile)) return;
  const entries = fs.readFileSync(logFile, 'utf8').split('\n').filter(Boolean).map((line) => JSON.parse(line));

  const count = (items) => {
    const tally = {};
    for (const item of items) tally[item] = (tally[item] || 0) + 1;
    return Object.entries(tally).sort((a, b) => b[1] - a[1]).map(([name, n]) => `${name} (${n})`).join(', ') || 'none';
  };
  const list = (items) => (items && items.length ? items.map((item) => `\`${item}\``).join(', ') : 'none');
  const cell = (text) => String(text ?? '').replace(/\|/g, '\\|').replace(/\n/g, ' ');

  const lines = [
    '# AI interaction log: readable summary',
    '',
    '_Generated from `ai-interactions.jsonl` by `.claude/hooks/render-summary.js`. Do not edit by hand._',
    '',
    '## Totals',
    '',
    `- Interactions logged: ${entries.length} (${entries.filter((e) => e.source === 'hook').length} captured live by the hook, ${entries.filter((e) => e.source === 'backfill').length} backfilled)`,
    `- Tool and model: ${count(entries.map((e) => `${e.ai_tool} / ${e.model}`))}`,
    `- Stages: ${count(entries.map((e) => e.stage))}`,
    `- Skills used: ${count(entries.flatMap((e) => (e.skills_used || []).map((s) => s.name)))}`,
    `- Sub-agents used: ${count(entries.flatMap((e) => (e.subagents_used || []).map((s) => s.name)))}`,
    `- MCP tools used: ${count(entries.flatMap((e) => (e.mcp_tools_used || []).map((m) => `${m.server}/${m.tool}`)))}`,
    `- Interactions with a recorded human decision: ${entries.filter((e) => e.human_decision).length} of ${entries.length}`,
    `- Outcomes: ${count(entries.map((e) => e.outcome || e.status))}`,
    '',
    '## Timeline',
    '',
    '| Time (UTC) | Stage | Request | Agent / mode | Outcome | Human decision | Verification |',
    '| --- | --- | --- | --- | --- | --- | --- |',
  ];

  for (const entry of entries) {
    lines.push(`| ${cell(entry.timestamp)} | ${cell(entry.stage)} | ${cell(entry.request).slice(0, 110)} | ${cell(`${entry.agent_name} (${entry.agent_type}), ${entry.mode}`)} | ${cell(entry.outcome || entry.status)} | ${cell(entry.human_decision || '-').slice(0, 140)} | ${cell((entry.action_verification || ['-']).join('; ')).slice(0, 160)} |`);
  }

  lines.push('', '## Detail', '');
  for (const entry of entries) {
    lines.push(`### ${entry.id}`, '');
    lines.push(`- **When:** ${entry.timestamp}${entry.completed_at ? ` to ${entry.completed_at}` : ''} (${entry.source})`);
    lines.push(`- **Tool / model / mode:** ${entry.ai_tool} / ${entry.model} / ${entry.mode}`);
    lines.push(`- **Request:** ${entry.request}`);
    lines.push(`- **Skills:** ${(entry.skills_used || []).map((s) => `${s.name} (${s.type}, ${s.invocation})`).join(', ') || 'none'}`);
    lines.push(`- **Sub-agents:** ${(entry.subagents_used || []).map((s) => `${s.name} (${s.type}, ${s.status}): ${s.task}`).join('; ') || 'none'}`);
    lines.push(`- **MCP tools:** ${(entry.mcp_tools_used || []).map((m) => `${m.server}/${m.tool}`).join(', ') || 'none'}`);
    lines.push(`- **Tools:** ${list(entry.tools_used)}`);
    lines.push(`- **Artifacts:** ${list(entry.artifacts)}`);
    lines.push(`- **Related commits:** ${list(entry.related_ids)}`);
    lines.push(`- **AI output / suggestion:** ${entry.suggestion || 'none'}`);
    lines.push(`- **Human decision / intervention:** ${entry.human_decision || 'none recorded'}`);
    lines.push(`- **Verification:** ${(entry.action_verification || []).join('; ') || 'none recorded'}`);
    lines.push(`- **Limitation:** ${entry.limitation || 'none recorded'}`);
    lines.push(`- **Outcome:** ${entry.outcome || entry.status}`, '');
  }

  fs.writeFileSync(path.join(root, 'docs', 'ai-log', 'ai-interactions-summary.md'), `${lines.join('\n')}\n`);
}

module.exports = { renderSummary };

if (require.main === module) renderSummary(process.env.CLAUDE_PROJECT_DIR || process.cwd());
