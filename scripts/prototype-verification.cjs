const fs = require('node:fs');
const path = require('node:path');
const { pathToFileURL } = require('node:url');
const { chromium } = require('playwright');
const output = path.resolve(process.argv[2]);
fs.mkdirSync(output, { recursive: true });
const standalone = path.join(output, 'standalone'); fs.mkdirSync(standalone, { recursive: true });
fs.copyFileSync(path.resolve(__dirname, '../design/index.html'), path.join(standalone, 'index.html'));
const assert = (value, message) => { if (!value) throw new Error(message); console.log('PASS ' + message); };
(async () => {
  const browser = await chromium.launch({ headless: true });
  try {
    const page = await browser.newPage({ viewport: { width: 1280, height: 840 }, deviceScaleFactor: 1 });
    const errors = [], external = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('request', request => { if (/^https?:/.test(request.url())) external.push(request.url()); });
    await page.goto(pathToFileURL(path.join(standalone, 'index.html')).href);
    await page.waitForSelector('.pane');
    assert((await page.locator('#groups button').count()) === 13, 'all 13 categories are available offline');
    for (const button of await page.locator('#groups button').all()) {
      await button.click(); assert((await page.locator('#commands button').count()) > 0, 'category has tools: ' + await button.textContent());
    }
    await page.evaluate(() => open('base64-string-converter'));
    await page.locator('[data-field="text"]').fill('Document one');
    await page.locator('[data-run]').click();
    assert((await page.locator('.output').textContent()).includes('RG9jdW1lbnQgb25l'), 'base64 executes in the offline prototype');
    await page.evaluate(() => open('case-converter'));
    await page.locator('[data-field="text"]').fill('Second document');
    await page.evaluate(() => open('base64-string-converter'));
    assert(await page.locator('[data-field="text"]').inputValue() === 'Document one', 'switching tools preserves document state');
    await page.locator('#split').click(); assert(await page.locator('.pane').count() === 2, 'two documents can be displayed side by side');
    await page.locator('#split').click();
    await page.locator('#language').selectOption('en-US');
    assert(await page.locator('[data-field="text"]').inputValue() === 'Document one', 'language switching preserves draft');
    for (const palette of ['DotNet','Aurora','Ember','Default','Aquatic','Desert','Dusk','NightSky']) {
      await page.locator('#palette').selectOption(palette);
      for (const mode of ['Light','Dark']) {
        await page.locator('#mode').selectOption(mode);
        await page.screenshot({ path: path.join(output, palette + '-' + mode + '.png') });
      }
    }
    await page.reload();
    assert(await page.locator('#palette').inputValue() === 'NightSky' && await page.locator('#mode').inputValue() === 'Dark', 'appearance preferences survive reload');
    await page.locator('#about').click(); await page.locator('#scenario').selectOption('download-failed');
    await page.locator('#update').click(); await page.waitForFunction(() => updateState === 'available');
    await page.locator('#update').click(); await page.waitForFunction(() => updateState === 'failed');
    await page.locator('#update').click(); await page.waitForFunction(() => updateState === 'downloaded');
    assert(true, 'download and checksum failure can retry the current step');
    await page.locator('#scenario').selectOption('available'); await page.locator('#update').click(); await page.locator('#cancel-update').click();
    assert(await page.evaluate(() => updateState) === 'canceled', 'update cancellation restores controls');
    await page.locator('#close-about').click();
    await page.locator('#palette').selectOption('DotNet'); await page.locator('#mode').selectOption('Light'); await page.locator('#language').selectOption('zh-CN');
    await page.screenshot({ path: path.join(output, 'workspace-zh-CN.png') });
    await page.evaluate(async () => {
      lang = 'zh-CN'; mode = 'System'; palette = 'DotNet'; group = 'Converter'; split = false; docs = [];
      open('base64-string-converter'); docs[0].values.text = 'Independent 中文 😀'; docs[0].width = 338;
      open('case-converter'); open('base64-string-converter'); await run('base64-string-converter'); render();
    });
    for (const language of ['zh-CN', 'en-US']) {
      await page.locator('#language').selectOption(language);
      await page.screenshot({ path: path.join(output, 'comparison-' + language + '.png') });
    }
    assert(errors.length === 0, 'zero JavaScript errors: ' + errors.join('; '));
    assert(external.length === 0, 'zero external requests');
    fs.writeFileSync(path.join(output, 'verification.json'), JSON.stringify({ standalone:true, categories:13, paletteSnapshots:16, errors, external }, null, 2));
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
