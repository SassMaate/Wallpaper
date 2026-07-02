# `.pages` — retired

The old Nuxt marketing site that used to live here has been **retired**. The
official website is now **https://sucrose.dev**.

This folder now contains only a static client-side redirect:

- `index.html` / `404.html` — meta-refresh + canonical + path-preserving
  `location.replace` that forwards `https://taiizor.github.io/Sucrose/*` to
  `https://sucrose.dev/*` (GitHub Pages cannot issue a server-side 301 to an
  external host).

It is published by `.github/workflows/redirect-deploy.yml`, which no longer
builds Nuxt — it just deploys this folder to GitHub Pages.

Keep the redirect live for an extended period (months) so search engines
complete the move. See `Hub/docs/runbook-retire-nuxt.md` for the full rationale.