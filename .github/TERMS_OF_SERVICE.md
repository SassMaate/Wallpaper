# Sucrose Terms of Service

Last updated: 3rd June 2026

These Terms of Service ("Terms") govern your use of the Sucrose application and its related online services (together, the "Services"). Sucrose is a free, open-source wallpaper engine for Windows developed and maintained by **Taiizor** ("we", "us", or "the Developer"). Please read these Terms carefully. By downloading, installing, or using Sucrose, you agree to be bound by them. If you do not agree, do not use the Services.

These Terms are an agreement about your **use of the Services**. They do not override the open-source license that governs the Sucrose software itself — see Section 3.

## 1. Definitions

- **"Software"** — the Sucrose desktop application and its source code, licensed under the GNU General Public License v3.0 (see Section 3).
- **"Online Services"** — the first-party backend operated by the Developer (`sucrose.soferity.com`), which powers the community Store, software updates, and optional telemetry/error reporting.
- **"Store"** — the community marketplace where users download and publish wallpapers.
- **"Wallpaper"** — content rendered by Sucrose, which may be of type Gif, Url, Web, Video, YouTube, or Application.
- **"User Content"** — wallpapers and other materials you create, configure, upload, or publish.

## 2. Acceptance and Eligibility

2.1. By using the Services you confirm that you are able to form a legally binding agreement and that you are at least 13 years of age, or the minimum age required to use software in your jurisdiction, whichever is higher.

2.2. The Store may contain content flagged as mature or adult. To view, download, or publish such content, you must be of the age of legal majority in your jurisdiction (typically 18). You are responsible for ensuring that your use of such content is lawful where you live (see Section 7.4).

2.3. If you use the Services on behalf of an organization, you represent that you are authorized to accept these Terms on its behalf.

## 3. The Software Is Open Source (GPL-3.0)

3.1. The Software is free and open-source, licensed under the **GNU General Public License, version 3 (GPL-3.0)**. Copyright © Taiizor. The full license text is distributed with the Software in the `LICENSE` file and at <https://www.gnu.org/licenses/gpl-3.0.html>.

3.2. The GPL-3.0 grants you the right to use, study, modify, and redistribute the Software under the terms of that license. **Nothing in these Terms limits or replaces your rights under the GPL-3.0 with respect to the Software.** In the event of a conflict between these Terms and the GPL-3.0 concerning the Software, the GPL-3.0 prevails for the Software.

3.3. These Terms additionally govern matters the GPL-3.0 does not address — in particular your use of the **Online Services and the Store**, your **User Content**, acceptable conduct, and the disclaimers and limitations in Sections 11–14.

3.4. Sucrose is provided at no cost. There are no paid licenses, subscriptions, trials, activation keys, or feature gates; all functionality is available to everyone (see Section 9).

## 4. The Services

4.1. **Local application.** Sucrose runs on your own device and renders wallpapers locally. It uses a multi-process architecture whose components communicate with each other only on your machine.

4.2. **Online Services.** When you use the Store, check for updates, or have diagnostic features enabled, Sucrose communicates with the Developer's backend at `sucrose.soferity.com`, and in some cases with GitHub. The handling of data is described in our [Privacy Policy](./PRIVACY_POLICY.md).

4.3. **Third-party platforms.** Sucrose is distributed through the Microsoft Store, GitHub Releases, and community package managers, and it can load content from third parties (such as YouTube or any website you choose). Your use of those platforms and services is governed by **their** own terms and policies, not these Terms.

4.4. **System requirements.** Some features require components supplied by Microsoft, such as the Visual C++ Redistributable and the WebView2 Runtime. These are third-party prerequisites obtained from Microsoft and are not part of the Software.

## 5. Wallpaper Types and Third-Party Content

5.1. Sucrose can render several wallpaper types, some of which load or run content that the Developer does not create, host, or control:

- **Web, Url, and YouTube** wallpapers load remote content from websites and services **you** choose. That content is subject to the third party's own terms and privacy practices.
- **Application** wallpapers run an external program (an executable) supplied by the wallpaper's author.
- **Gif, Video,** and other local wallpapers render files you provide.

5.2. You are responsible for the legality and safety of any URL, website, file, or program you use as a wallpaper, and for complying with the terms of any third-party service you direct Sucrose to access.

## 6. Important Security Notice: Running Third-Party Wallpapers

**Please read this section carefully.**

6.1. **Application wallpapers execute code on your computer.** When you use an Application-type wallpaper — whether you created it yourself or obtained it from the Store or another source — Sucrose launches the associated program with your normal user privileges.

6.2. **Sucrose does not sandbox, code-sign, scan, or otherwise vet third-party wallpapers.** Downloaded wallpaper packages are extracted and may be launched without any malware scanning, signature verification, or security review by the Developer. Application wallpapers may also run with command-line arguments specified by their author.

6.3. **Treat community and third-party Application wallpapers like any other untrusted program from the internet.** They can access your files and system to the same extent any program you run can. You are solely responsible for evaluating the source, author, and safety of a wallpaper before using it.

6.4. To the maximum extent permitted by law, the Developer is **not responsible** for any harm — including malware, data loss, system damage, or unauthorized access — caused by third-party or community wallpapers. Use wallpapers only from sources you trust, and keep appropriate security software in place.

## 7. The Community Store and User Content

7.1. **Downloading.** You may download wallpapers other users have published. They are User Content created by third parties; the Developer does not endorse, guarantee, or assume responsibility for them (see Section 6).

7.2. **Publishing.** Publishing a wallpaper to the Store is always initiated by you. When you publish, the wallpaper package (a `.zip` of up to 90 MB, with required metadata such as `SucroseInfo.json`) is uploaded to the Online Services and made **publicly available** for others to download. Do not include personal, confidential, or sensitive information in anything you publish.

7.3. **Your rights and the license you grant.** You retain ownership of and any license you attach to the wallpapers you create. By publishing a wallpaper, you grant the Developer a worldwide, non-exclusive, royalty-free license to host, store, reproduce, distribute, and publicly make available that wallpaper through the Store, solely for the purpose of operating and promoting the Services. You may request removal of your published wallpaper at any time (see Section 7.6); removal does not affect copies others have already downloaded.

7.4. **Your warranties.** For everything you publish, you represent and warrant that: (a) you own it or have all rights and permissions necessary to share it; (b) it does not infringe any copyright, trademark, privacy, publicity, or other right of any third party; (c) it contains no malware, harmful code, or hidden functionality; and (d) it complies with these Terms and with applicable law, including any age or content-rating requirements where mature content is involved.

7.5. **Content standards.** You must not create, upload, publish, or distribute content that:

   a) is unlawful, or promotes or facilitates illegal or malicious activity;
   b) contains viruses, malware, spyware, or any harmful or deceptive code;
   c) infringes the intellectual-property, privacy, or other rights of others;
   d) is sexual content involving minors, or sexualizes minors in any way (strictly prohibited, with zero tolerance);
   e) harasses, threatens, defames, or incites violence or hatred against any person or group;
   f) is spam, fraudulent, deceptive, or misleading; or
   g) exposes the personal information of others without consent.

   Mature or adult content that is otherwise lawful may be permitted only where it is clearly and accurately marked as such, in accordance with the Store's labeling and the eligibility rules in Section 2.2.

7.6. **Reporting and moderation.** Users can report a wallpaper for reasons including spam, nudity, harmful content, violence, malicious code, copyright violation, misleading information, or other concerns. Reports may be submitted in-app or via the project's issue tracker. We may, at our discretion and without obligation to monitor content proactively, review reports and **remove content, refuse uploads, or limit access** to the Online Services for content or behavior that violates these Terms. Because the Services do not use accounts, enforcement may be applied at the level of a device or upload.

7.7. **Copyright claims.** If you believe content in the Store infringes your copyright or other rights, contact us using the details in Section 15 with enough information to identify the content and your claim. We will review valid claims and remove infringing content where appropriate.

## 8. Acceptable Use

8.1. You agree not to:

   a) use the Services for any unlawful purpose or in violation of these Terms;
   b) interfere with, disrupt, overload, or attempt to gain unauthorized access to the Online Services or their infrastructure;
   c) abuse the Store or telemetry endpoints, including by automated mass uploads, spoofing, or circumventing upload quotas or moderation; or
   d) misrepresent your identity or your authority to publish content.

8.2. Your rights to use, modify, and redistribute the **Software** under the GPL-3.0 are not restricted by this Section; it concerns use of the **Online Services and Store**.

## 9. Fees and Donations

9.1. Sucrose is free of charge. The Developer does not sell the Software or any feature, and there are no in-app purchases.

9.2. The application includes an optional way to support development through voluntary donations. Donations are entirely optional and **do not unlock any features** — all functionality is available whether or not you donate. The in-app "Advertising" setting referred to in the donation area controls only whether Sucrose displays its own support/donation prompt; it is **not** third-party advertising.

## 10. Privacy

Your use of the Services is also governed by our [Privacy Policy](./PRIVACY_POLICY.md), which explains what information Sucrose handles. Note that some diagnostic features — "Share Usage Statistics" and "Automatic Error Reporting" — are **enabled by default** and can be turned off at any time in **Settings → Other**, as described in the Privacy Policy.

## 11. Disclaimer of Warranties

11.1. The Services are provided **"AS IS" and "AS AVAILABLE", without warranties of any kind**, whether express, implied, or statutory, including any implied warranties of merchantability, fitness for a particular purpose, title, and non-infringement. This is consistent with, and in addition to, the warranty disclaimer in the GPL-3.0 that applies to the Software.

11.2. We do not warrant that the Services will be uninterrupted, secure, or error-free, that the Online Services will always be available, or that any wallpaper or content obtained through the Store is safe, lawful, accurate, or fit for any purpose.

11.3. You acknowledge the security notice in Section 6: third-party and Application wallpapers are not vetted by the Developer and are used at your own risk.

## 12. Limitation of Liability

12.1. To the maximum extent permitted by applicable law, the Developer shall not be liable for any indirect, incidental, special, consequential, exemplary, or punitive damages, or for any loss of data, profits, goodwill, or business, arising out of or relating to your use of (or inability to use) the Services or any User Content, even if advised of the possibility of such damages.

12.2. To the maximum extent permitted by applicable law, the Developer's total aggregate liability arising out of or relating to the Services shall not exceed the amount you paid to use them — which for a free application is **zero**.

12.3. Nothing in these Terms excludes or limits liability that cannot be excluded or limited under applicable law.

## 13. Indemnification

To the extent permitted by law, you agree to indemnify and hold harmless the Developer from any claims, damages, liabilities, and reasonable expenses arising out of (a) your User Content, (b) your use of the Services, or (c) your violation of these Terms or of any third-party right.

## 14. Intellectual Property

14.1. **The Software** is open-source under the GPL-3.0 (Section 3). It is **not** proprietary to the Developer in the sense of restricting your GPL rights; you are free to fork, modify, and redistribute it under that license. The Software incorporates third-party libraries and components (for example, the Skylark libraries, WebView2, and CefSharp), each of which remains the property of its respective owners and is used under its own license.

14.2. **Your User Content** remains yours. Each wallpaper may carry its own license and author information in its metadata; you are responsible for licensing your wallpapers appropriately. Publishing grants the limited distribution license described in Section 7.3.

14.3. **Names and branding.** The "Sucrose" name, logo, and associated branding identify the project. Trademark and brand rights are not granted by the GPL-3.0; please do not use them in a way that implies endorsement by or affiliation with the Developer without permission.

## 15. Suspension and Changes to the Services

15.1. The Developer may modify, suspend, or discontinue any part of the Online Services or Store at any time, including for maintenance, legal, or security reasons. Because the Software is GPL-3.0 and runs locally, you may continue to use a copy you already have, subject to the availability of the Online Services it relies on.

15.2. We may restrict or revoke access to the Online Services for content or conduct that violates these Terms, as described in Section 7.6.

## 16. Changes to These Terms

We may update these Terms from time to time as the project evolves or as legal requirements change. Material changes will be reflected by updating the "Last updated" date above and publishing the revised Terms with the Software and in our repository. Your continued use of the Services after changes take effect constitutes acceptance of the updated Terms.

## 17. Governing Law and Severability

17.1. These Terms are governed by the laws applicable in the Developer's place of residence, without regard to conflict-of-law principles, except where mandatory consumer-protection laws of your own country of residence apply.

17.2. If any provision of these Terms is found unenforceable, the remaining provisions remain in full force, and the unenforceable provision will be applied to the maximum extent permissible.

17.3. These Terms, together with the Privacy Policy and the GPL-3.0 (with respect to the Software), constitute the entire agreement between you and the Developer regarding the Services and supersede any prior understandings on that subject.

## 18. Contact

If you have questions about these Terms, please contact us:

- Email: taiizor@vegalya.com
- Website: https://taiizor.github.io/Sucrose
- Project: https://github.com/Taiizor/Sucrose
