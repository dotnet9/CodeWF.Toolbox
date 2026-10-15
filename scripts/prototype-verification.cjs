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
    const selectPalette=async value=>page.locator('[data-title-palette="'+value+'"]').click();
    const selectMode=async value=>{for(let i=0;i<3&&await page.locator('#title-mode').getAttribute('data-mode')!==value;i++)await page.locator('#title-mode').click();};
    const selectLanguage=async value=>{await page.locator('#title-language summary').click();await page.locator('[data-title-language="'+value+'"]').click();};
    const errors = [], external = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('request', request => { if (/^https?:/.test(request.url())) external.push(request.url()); });
    await page.goto(pathToFileURL(path.join(standalone, 'index.html')).href);
    await page.waitForSelector('.pane');
    assert(await page.locator('.desktop-titlebar [data-window]').count()===3,'three desktop caption buttons');
    assert(await page.locator('.desktop-window').evaluate(el=>parseFloat(getComputedStyle(el).borderTopWidth)>=1),'visible outer desktop border');
    assert(await page.locator('#title-palettes button').count()===8,'eight flat title palette buttons');
    const swatches=()=>page.locator('[data-title-palette]').evaluateAll(items=>items.map(button=>({key:button.dataset.titlePalette,color:getComputedStyle(button.querySelector('i')).backgroundColor,image:getComputedStyle(button.querySelector('i')).backgroundImage})));
    const fixedSwatches=await swatches(),expectedColors=['rgb(81, 43, 212)',null,'rgb(189, 103, 77)','rgb(0, 100, 250)','rgb(0, 141, 158)','rgb(181, 117, 50)','rgb(122, 92, 196)','rgb(43, 108, 176)'];
    assert(JSON.stringify(fixedSwatches.map(item=>item.key))===JSON.stringify(['DotNet','Aurora','Ember','Default','Aquatic','Desert','Dusk','NightSky'])&&!fixedSwatches.some((item,index)=>index===1?!item.image.includes('rgb(137, 104, 255), rgb(38, 212, 204)'):item.color!==expectedColors[index]),'StudioSwatch .21 fixed color values, gradient and order');
    const normalWidth=await page.locator('.desktop-window').evaluate(el=>el.getBoundingClientRect().width);
    await page.locator('[data-window=maximize]').click();assert(await page.locator('.desktop-window').evaluate(el=>el.getBoundingClientRect().width)>normalWidth,'maximize changes window geometry');await page.locator('[data-window=maximize]').click();assert(await page.locator('.desktop-window').evaluate(el=>el.getBoundingClientRect().width)===normalWidth,'restore recovers geometry');
    for(const action of ['minimize','close']){await page.locator('[data-window='+action+']').click();assert(!await page.locator('.desktop-window').isVisible()&&await page.locator('#restore-window').isVisible(),'window actually hides: '+action);await page.locator('#restore-window').click();assert(await page.locator('.desktop-window').isVisible(),'restore/reopen is clickable: '+action);}
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
    assert(await page.locator('[data-title-language]').count()===4,'language icon contains all four production languages');
    for(const language of ['zh-CN','zh-Hant','en-US','ja-JP']) {
      await selectLanguage(language);
      const languageLabel=await page.evaluate(()=>DATA.dictionaries[lang]['Localization.CommonSettingView.LanguageKey']);
      assert(await page.locator('#title-language summary').getAttribute('title')===languageLabel&&await page.locator('#title-language summary').getAttribute('aria-label')===languageLabel,'localized language tooltip and accessible name: '+language);
      assert(!await page.locator('#title-language').evaluate(el=>el.open)&&await page.locator('[data-title-language="'+language+'"]').getAttribute('aria-checked')==='true','language selection closes menu and updates checked state');
      assert(JSON.stringify(await page.locator('[data-title-language]').allTextContents())===await page.evaluate(()=>JSON.stringify(DATA.languages.map(item=>item.description))),'language menu uses production descriptions');
      assert(await page.locator('[data-window=close]').getAttribute('aria-label')===({'zh-CN':'关闭','zh-Hant':'關閉','en-US':'Close','ja-JP':'閉じる'})[language],'localized caption names: '+language);
      await page.locator('#title-language summary').click();await page.locator('#title-language summary').press('Escape');
      assert(!await page.locator('#title-language').evaluate(el=>el.open),'Escape closes language menu');
      await page.locator('#about').click();
      assert(await page.locator('#about-title').textContent()===await page.evaluate(()=>DATA.dictionaries[lang]['Localization.AboutView.Title']),'About uses actual resources: '+language);
      assert(await page.locator('#about-version').textContent()===await page.evaluate(()=>DATA.metadata.version),'About version comes from production metadata');
      await page.screenshot({path:path.join(output,'about-'+language+'.png')});await page.locator('#close-about').click();
    }
    await selectLanguage('en-US');
    assert(await page.locator('[data-field="text"]').inputValue() === 'Document one', 'language switching preserves draft');
    for (const palette of ['DotNet','Aurora','Ember','Default','Aquatic','Desert','Dusk','NightSky']) {
      await selectPalette(palette);
      for (const mode of ['Light','Dark']) {
        await selectMode(mode);
        assert(JSON.stringify(await swatches())===JSON.stringify(fixedSwatches),'palette dots remain fixed: '+palette+' '+mode);
        await page.screenshot({ path: path.join(output, palette + '-' + mode + '.png') });
      }
    }
    await page.reload();
    assert(await page.locator('#palette').inputValue() === 'NightSky' && await page.locator('#mode').inputValue() === 'Dark', 'appearance preferences survive reload');
    await page.locator('#about').click();
    assert(await page.locator('#about-dialog .dialog-caption button').count()===3,'About has three window buttons');
    assert(await page.locator('#about-build').textContent()==='2026-10-15 00:00:00','fixed prototype build timestamp');
    assert(await page.locator('#about-dialog .dialog-top').evaluate(el=>el.getBoundingClientRect().height)===40,'desktop About titlebar');
    await page.screenshot({path:path.join(output,'about-desktop-en-US.png')});
    await page.locator('#scenario').selectOption('download-failed');
    await page.locator('#update').click(); await page.waitForFunction(() => updateState === 'available');
    await page.locator('#update').click(); await page.waitForFunction(() => updateState === 'failed');
    await page.locator('#update').click(); await page.waitForFunction(() => updateState === 'downloaded');
    assert(true, 'download and checksum failure can retry the current step');
    await page.locator('#scenario').selectOption('available'); await page.locator('#update').click(); await page.locator('#cancel-update').click();
    assert(await page.evaluate(() => updateState) === 'canceled', 'update cancellation restores controls');
    await page.locator('#close-about').click();
    await selectPalette('DotNet'); await selectMode('Light'); await selectLanguage('zh-CN');
    await page.screenshot({ path: path.join(output, 'workspace-zh-CN.png') });
    await page.evaluate(async () => {
      lang = 'zh-CN'; mode = 'System'; palette = 'DotNet'; group = 'Converter'; split = false; docs = [];
      open('base64-string-converter'); docs[0].values.text = 'Independent 中文 😀'; docs[0].width = 338;
      open('case-converter'); open('base64-string-converter'); await run('base64-string-converter'); render();
    });
    for (const language of ['zh-CN', 'en-US']) {
      await selectLanguage(language);
      await page.screenshot({ path: path.join(output, 'comparison-' + language + '.png') });
    }
    await page.setViewportSize({width:980,height:840});
    assert(await page.locator('.desktop-titlebar').evaluate(el=>{const search=el.querySelector('#search').getBoundingClientRect(),controls=el.querySelector('.studio-title-controls').getBoundingClientRect(),caption=el.querySelector('.window-controls').getBoundingClientRect();return search.right<=controls.left&&controls.right<=caption.left&&caption.right<=el.getBoundingClientRect().right;}),'980px title controls do not overlap');
    await page.screenshot({path:path.join(output,'desktop-980.png')});
    assert(errors.length === 0, 'zero JavaScript errors: ' + errors.join('; '));
    assert(external.length === 0, 'zero external requests');
    fs.writeFileSync(path.join(output, 'verification.json'), JSON.stringify({ standalone:true, categories:13, paletteSnapshots:16, errors, external }, null, 2));
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
