// 仅用于重新生成已提交背景；产品打包只读取 PNG，不依赖 Node 或浏览器。
const fs=require('node:fs'),path=require('node:path');
const {chromium}=require('playwright');
const languages={
  bilingual:['准备安装 · Ready to install','拖动应用到 Applications · Drag the app to Applications','打开应用 · Open app','完成复制后，从 Applications 启动 · Launch from Applications after copying'],
  zh:['准备安装','将应用拖入 Applications 文件夹','打开应用','复制完成后，从 Applications 启动应用'],
  en:['Ready to install','Drag the app into the Applications folder','Open app','After copying, launch the app from Applications']
};
function svg([heading,instruction,open,footer]){return `<svg xmlns="http://www.w3.org/2000/svg" width="720" height="480" viewBox="0 0 720 480">
<defs><linearGradient id="purple" x2="1" y2="1"><stop stop-color="#673FE1"/><stop offset=".6" stop-color="#512BD4"/><stop offset="1" stop-color="#32127E"/></linearGradient><linearGradient id="surface" x2="0" y2="1"><stop stop-color="#FDFBFF"/><stop offset="1" stop-color="#EEE8FB"/></linearGradient><linearGradient id="core" x2="1" y2="1"><stop stop-color="#8D6BFF"/><stop offset="1" stop-color="#512BD4"/></linearGradient></defs>
<rect width="720" height="480" fill="url(#surface)"/>
<path d="M0 0H720V104L692 132H28L0 104Z" fill="url(#purple)"/>
<g fill="none" stroke="#D8CEEF"><ellipse cx="360" cy="250" rx="313" ry="130" transform="rotate(-9 360 250)"/><ellipse cx="360" cy="250" rx="278" ry="113" transform="rotate(12 360 250)" stroke-dasharray="2 8"/><path d="M0 420L88 333M720 420L633 333"/></g>
<g fill="#AE9AD9"><circle cx="58" cy="268" r="3"/><circle cx="626" cy="183" r="3"/><circle cx="664" cy="282" r="2"/><circle cx="118" cy="363" r="2"/></g>
<g font-family="Segoe UI,Arial,Microsoft YaHei,sans-serif"><text x="32" y="37" fill="#DBD0FF" font-size="10" letter-spacing="2">CodeWF / macOS</text><text x="32" y="73" fill="white" font-size="24" font-weight="600">${heading}</text><text x="32" y="100" fill="#E2D9FF" font-size="11">NATIVE APPLICATION · CodeWF</text></g>
<g transform="translate(642 68)"><g fill="none" stroke="#D8C9FF" opacity=".6"><ellipse rx="54" ry="22" transform="rotate(-23)"/><circle r="48" stroke-dasharray="1 5"/></g><path d="M-19-34H19L34-19V19L19 34H-19L-34 19V-19Z" fill="#9471F6"/><path d="M-17-29H17L29-17V17L17 29H-17L-29 17V-17Z" fill="url(#core)"/><text x="0" y="4" fill="white" text-anchor="middle" font-size="12" font-weight="700" font-family="Segoe UI,Arial,sans-serif">CodeWF</text><circle cx="-45" cy="-25" r="3" fill="#E6DCFF"/></g>
<g fill="#FFFFFF" fill-opacity=".72" stroke="#D4C7EE"><path d="M118 166H242L264 188V282L242 304H118L96 282V188Z"/><path d="M478 166H602L624 188V282L602 304H478L456 282V188Z"/></g>
<g fill="none" stroke="#512BD4" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M309 235H402M389 222L402 235L389 248"/><path d="M316 244H373" stroke-width="1" opacity=".25"/></g>
<g font-family="Segoe UI,Arial,Microsoft YaHei,sans-serif" text-anchor="middle"><text x="360" y="354" fill="#3A216B" font-size="15" font-weight="600">${instruction}</text><text x="540" y="381" fill="#6A598B" font-size="11">${open}</text></g>
<path d="M24 400H696" stroke="#D4C7EA"/><circle cx="32" cy="430" r="3" fill="#512BD4"/><text x="45" y="434" fill="#65527F" font-size="10" font-family="Segoe UI,Arial,Microsoft YaHei,sans-serif">${footer}</text><text x="690" y="434" fill="#65527F" font-size="11" text-anchor="end" font-family="Consolas,monospace">codewf.com</text>
</svg>`;}
(async()=>{const browser=await chromium.launch({headless:true});try{const page=await browser.newPage({viewport:{width:720,height:480},deviceScaleFactor:1});for(const [language,copy]of Object.entries(languages)){const source=svg(copy);fs.writeFileSync(path.join(__dirname,`background-${language}.svg`),source+'\n');await page.setContent('<style>html,body{margin:0;width:720px;height:480px;overflow:hidden}</style>'+source);await page.screenshot({path:path.join(__dirname,`background-${language}.png`)});}}finally{await browser.close();}})().catch(error=>{console.error(error);process.exitCode=1;});
