# Privacy Policy

Last updated: 3rd June 2026

Welcome, and thank you for choosing Sucrose (hereinafter referred to as the "App"). Sucrose is a free and open-source wallpaper engine for Windows that supports Gif, Url, Web, Video, YouTube, and Application wallpapers, together with audio-reactive and system-status features. We care about your privacy, and this Privacy Policy explains what information the App handles, how it is used, where it is stored, and the choices you have. Please read it carefully. By continuing to use the App, you agree to the practices described here.

A guiding principle: **most of what Sucrose reads from your system never leaves your device.** The only information sent to our servers is diagnostic, usage, and error data — and that data is gated by settings you can turn off.

## 1. Information You Provide

- **Wallpapers and content you create.** Wallpapers you create, configure, and use stay on your device. If you choose to **publish a wallpaper to the community Store**, the wallpaper package (a compressed `.zip`, up to 90 MB) is uploaded to our servers and becomes publicly available to other users. Uploading is always a deliberate, user-initiated action — nothing you create is published automatically. See Section 5.
- **Optional GitHub Personal Access Token.** You may optionally provide a GitHub Personal Access Token to raise GitHub API rate limits when browsing the GitHub-hosted Store. It is used only as an authorization header for GitHub requests, is stored locally on your device, and is never sent to our own servers.

## 2. Information Collected Automatically

The App can send a limited set of diagnostic and usage information to our first-party backend at `https://sucrose.soferity.com` ("Soferity", operated by the App's developer — **not** a third-party analytics or advertising provider). Sucrose does **not** integrate any third-party analytics, tracking, or advertising SDKs (such as Google Analytics, Sentry, Mixpanel, App Center, Segment, Firebase, or any ad network).

### 2.1. Diagnostic and Usage Data (Telemetry)

When the **"Share Usage Statistics"** option is enabled, the App periodically sends the following to our servers:

- **Device and system information:** device model and manufacturer, processor name(s)/core count, graphics adapter name(s), network adapter name(s), total memory, and operating system name, version, build, and architecture.
- **Application information:** App version, framework, and architecture.
- **Configuration and feature usage:** your App settings and preferences (for example, the wallpaper engine types you use, performance options, cycling and playback settings, theme type, Store preferences, update preferences, and Discord status). No wallpaper file contents are sent.
- **Locale:** your culture/language setting.
- **Online status:** a periodic "still running" heartbeat containing only the App version and session duration.
- **Wallpaper download events:** when you download a wallpaper from the Store, its title, version, and location are recorded together with the App version.

This data is associated with a **pseudonymous device identifier** (see Section 2.4) and not with a name or account.

### 2.2. Error and Crash Reports

When the **"Automatic Error Reporting"** option is enabled, the App sends crash and error reports to our servers so that problems can be diagnosed and fixed. A report may include:

- The full exception details, including the **error message and stack trace**. Stack traces can contain file paths, which on Windows may include your Windows user name (for example, `C:\Users\<your-name>\...`).
- Diagnostic context "breadcrumbs" describing what the App was doing.
- Your Windows user name, device model and manufacturer, operating system name/build, processor and process architecture, App version, locale, and the pseudonymous device identifier.

Crash reports do **not** include screenshots. A report is first written to a local cache file on your device and is deleted after it has been successfully sent.

### 2.3. Data Processed Only on Your Device

To render wallpapers and adapt to your system, the App reads additional information that **stays on your device and is not transmitted** to us or anyone else:

- **System monitoring:** real-time CPU and GPU usage, memory, battery state, storage, and network throughput, used to pause or optimize wallpapers (for example, when on battery or in full-screen apps).
- **Screen resolution and monitor layout,** used to position wallpapers and detect full-screen applications.
- **Audio (for audio-reactive wallpapers):** system audio is captured locally through Windows loopback and converted in memory into a frequency spectrum used to animate wallpapers. **No audio is recorded, stored, or transmitted.**

> Note: some device attributes (such as device model, operating system, and adapter names) are also included in telemetry/error reports when those features are enabled, as described in Sections 2.1 and 2.2.

### 2.4. Pseudonymous Device Identifier

The App does **not** have user accounts, logins, or passwords. To distinguish installations for diagnostics, abuse prevention, and Store quotas, requests to our backend include a pseudonymous identifier derived from your device (a value generated from your Windows user name, computer model, and machine identifier). It is used as an internal reference only.

## 3. Defaults and How to Turn Collection Off

**"Share Usage Statistics"** and **"Automatic Error Reporting"** are **enabled by default**. You can disable either of them at any time in the App under **Settings → Other**. When disabled, the corresponding data is not sent. No telemetry or error data is sent while the App is offline.

## 4. Purposes of Use

The information described above is used to:

- Provide, maintain, and improve the App's features and stability;
- Diagnose crashes and troubleshoot problems;
- Understand which features are used so we can prioritize development;
- Operate the community Store (downloads, uploads, and abuse/quota controls);
- Comply with applicable legal obligations.

Sucrose does **not** use your data to serve advertisements and does **not** build personalized advertising or content profiles. (The "Advertising" toggle inside the App's Donate settings only controls whether Sucrose shows its own in-app donation/support prompt; it is not third-party advertising.)

## 5. The Community Store

The App includes a Store for downloading and sharing wallpapers, served from our Soferity backend (with GitHub available as an alternative source).

- **Downloading** transmits the request and the download-event data described in Section 2.1.
- **Uploading / Publishing** is initiated only by you. The selected wallpaper package (`.zip`, up to 90 MB), together with a category and version, is uploaded to our servers and **published publicly** for other users to download. Please do not include personal or sensitive information in wallpapers you publish, as they become publicly accessible.
- **Reporting** a wallpaper sends your report and the pseudonymous identifier to our servers.

## 6. Third-Party Content in Wallpapers (Web, URL, and YouTube)

Web, URL, and YouTube wallpapers display remote content of **your** choosing inside an embedded browser engine (Microsoft WebView2 or Chromium/CefSharp):

- This content is loaded directly from the third-party sites you select (for example, **YouTube/Google** for YouTube wallpapers, or any website you enter as a URL wallpaper). Those sites operate under **their own** privacy policies and may collect data or set cookies independently of Sucrose. Sucrose itself does not transmit your browsing activity to its own servers.
- The embedded browser engines store a **local cache, cookies, and site data** on your device (under your `%APPDATA%\Sucrose\Cache` folder) so that web wallpapers work correctly. This data persists on your device and can be cleared by deleting the App's cache folder.

If you prefer not to expose yourself to a third party's tracking, avoid YouTube/remote-URL wallpapers or point Web wallpapers at local files.

## 7. Discord Rich Presence

If you have the Discord desktop client running and the integration enabled, the App displays a Discord "Rich Presence" status. This status uses **generic, predefined text and images only** — it does **not** reveal your wallpaper names, settings, or any personal data — along with the time you started the App. Communication happens locally between Sucrose and your Discord client; Discord then displays your status according to **Discord's** own privacy policy. You can disable this integration in **Settings → Other**.

## 8. Software Updates

The App can check for updates automatically. Update checks contact GitHub and/or our Soferity backend to retrieve release information, and when telemetry is enabled, an update check may include the current App version and your update preference. Update packages are downloaded (from GitHub releases or our backend) and applied locally.

## 9. Local Data Storage

The following are stored **locally on your device** and are not transmitted unless explicitly described above:

- App settings and preferences (including your telemetry and error-reporting choices);
- Downloaded wallpapers and Store cache (manifests, thumbnails);
- Embedded-browser cache, cookies, and site data for Web/URL/YouTube wallpapers;
- Local inter-process communication files used by the App's components;
- Crash reports staged for upload (deleted after sending) and temporary update files.

The App's multiple components communicate with each other entirely **on your local machine** (via named pipes, local files, and loopback connections); this internal communication does not send any data over the internet.

## 10. How We Share Information and Third-Party Services

We do not sell your personal information, and we do not share it except as described here:

- **Soferity backend (`sucrose.soferity.com`)** — our first-party server that receives telemetry, error reports, online status, download events, update checks, and Store uploads/reports, as described above.
- **GitHub** (`github.com`, `api.github.com`, `raw.githubusercontent.com`) — used as an alternative Store/update source, for release downloads, and for issue reporting.
- **Google / YouTube** — contacted only when you use a YouTube wallpaper, under Google's own privacy policy.
- **Websites you choose** — contacted only when you use Web/URL wallpapers, under their own privacy policies.
- **Discord** — for the optional Rich Presence status, under Discord's own privacy policy.
- **Microsoft** — for downloading required runtime components (such as the Visual C++ Redistributable and the WebView2 runtime) and for the Microsoft Store listing.

We may also disclose information where required to comply with applicable law or to protect our rights, users, or the public.

## 11. Data Security

We take reasonable technical measures to protect the limited data the App transmits, and transmissions to our backend use HTTPS. However, no method of transmission or storage over the internet is completely secure, and we cannot guarantee absolute security.

## 12. Data Retention and Your Rights

Diagnostic, usage, and error data are retained only as long as needed for the purposes described above. Because the App uses only a pseudonymous device identifier and has no user accounts, requests about your data should reference that context. Subject to applicable law, you may ask us to access or delete data associated with your device, or ask questions about how it is processed, by contacting us using the details in Section 16. You can also stop all telemetry and error reporting at any time as described in Section 3.

## 13. Children's Privacy

The App is a general-purpose desktop tool and is not directed at children. We do not ask for, require, or verify your age, and we have no way to determine the age of any user. We do not knowingly or intentionally collect personal information from children. If you are a parent or legal guardian and believe a child has provided personal information through the App — for example, by publishing a wallpaper to the Store — please contact us using the details in Section 16 and we will remove it. Parents and guardians are encouraged to monitor and manage their children's online activities.

## 14. Links and Third-Party Sites

The App and its wallpapers may contain links to, or display content from, third-party websites and services. We are not responsible for the privacy practices or content of those third parties. We encourage you to review their privacy policies.

## 15. Changes to This Privacy Policy

We may update this Privacy Policy from time to time as the App evolves or as legal requirements change. Updates are published with the App and in our repository, and the "Last updated" date above will be revised accordingly. We recommend reviewing this policy periodically.

## 16. Contact

If you have any questions or concerns about this Privacy Policy, please contact us at:

- Email: taiizor@vegalya.com
- Website: https://taiizor.github.io/Sucrose
- Project: https://github.com/Taiizor/Sucrose