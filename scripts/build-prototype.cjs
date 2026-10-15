const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '..');
const read = p => fs.readFileSync(path.join(root, p), 'utf8').replace(/^\uFEFF+/, '');
const catalog = read('src/CodeWF.Modules.ToolFramework/Services/ToolCatalog.cs');
const matches = [...catalog.matchAll(/Spec\("([^"]+)",\s*"([^"]+)",\s*"([^"]+)",\s*"([^"]+)"/g)];
const dictionaries = {};
const languages = ['zh-CN', 'zh-Hant', 'en-US', 'ja-JP'];
for (const language of languages) {
  dictionaries[language] = {};
  for (const project of fs.readdirSync(path.join(root, 'src'))) {
    const file = path.join(root, 'src', project, 'I18n', language + '.json');
    if (!fs.existsSync(file)) continue;
    function flatten(value, prefix = '') {
      for (const [key, item] of Object.entries(value)) {
        const name = prefix ? prefix + '.' + key : key;
        if (item && typeof item === 'object') flatten(item, name);
        else dictionaries[language][name] = item;
      }
    }
    flatten(JSON.parse(fs.readFileSync(file, 'utf8').replace(/^\uFEFF+/, '')));
  }
}
const key = value => value.replace(/[^\p{L}\p{N}]+/gu, '_').replace(/_$/, '');
let tools = matches.map((match, index) => {
  const block = catalog.slice(match.index, matches[index + 1]?.index ?? catalog.indexOf('private static ToolSpec'));
  const resource = 'Localization.ToolboxTools.Tools.' + key(match[1]);
  return { id: match[1], group: match[2], title: resource + '.Title', description: resource + '.Description',
    fields: [...block.matchAll(/\b(Text|Multi|Number|Bool|Select|File|SaveFile)\("([^"]+)",\s*"([^"]+)"/g)].map(field => ({id:field[2], kind:field[1], label:resource+'.Fields.'+key(field[2])})) };
});
const custom = [
  ['DateTimeConverterView','Converter','DateTimeConverterView'],['Base64CodecView','Converter','Base64CodecView'],
  ['GuidGeneratorView','Converter','GuidGeneratorView'],['YamlToJsonView','Converter','YamlToJsonView'],
  ['JsonToYamlView','Converter','JsonToYamlView'],['ImageToIconView','Converter','ImageToIconView'],['NuoCheView','Converter','NuoCheView'],
  ['YamlPrettifyView','Development','YamlPrettifyView'],['JsonPrettifyView','Development','JsonPrettifyView'],
  ['LogViewerView','Logs','LogViewerView'],['MergeXmlFilesView','XML','MergeXmlFilesView'],['ManageXmlFilesView','XML','ManageXmlFilesView']
];
for (const [id, group, resource] of custom) tools.push({id,group,title:`Localization.${resource}.Title`,description:`Localization.${resource}.Description`,fields:[{id:'input',kind:'Multi',label:'Localization.Workspace.Properties'}]});
const manifest = path.join(root, 'design/tool-manifest.json');
if (fs.existsSync(manifest)) tools = JSON.parse(fs.readFileSync(manifest, 'utf8').replace(/^\uFEFF+/, ''));
const palettes = JSON.parse(read('design/palettes.json'));
const logo = fs.readFileSync(path.join(root, 'logo.png')).toString('base64');
const props = read('Directory.Build.props');
const metadata = {version: props.match(/<Version>([^<]+)<\/Version>/)[1], build: '2026-10-15 00:00:00'};
const languageOptions = languages.map(cultureName => { const resource=JSON.parse(read('src/CodeWF.Toolbox/I18n/'+cultureName+'.json'));return {cultureName,description:resource.description}; });
const data = JSON.stringify({tools,dictionaries,palettes,logo,metadata,languages:languageOptions}).replace(/</g, '\\u003c');
const html = read('design/prototype.template.html').replace(/\r\n?/g, '\n').replace('/*__PROTOTYPE_DATA__*/', 'const DATA = ' + data + ';');
const target = path.join(root, 'design/index.html');
if (process.argv.includes('--check')) {
  if (!fs.existsSync(target) || fs.readFileSync(target, 'utf8').replace(/\r\n?/g, '\n') !== html) throw new Error('Prototype is stale; run node scripts/build-prototype.cjs');
} else fs.writeFileSync(target, html);
console.log(`Standalone prototype built: ${tools.length} tools, 13 groups, 16 palette variants.`);
