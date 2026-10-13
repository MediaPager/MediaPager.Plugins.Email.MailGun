# MediaPager.Plugins.Email.MailGun

Official Mailgun email provider plugin for [MediaPager](https://github.com/MediaPager/MediaPager).
Ships with the app and is **loaded by default**.

Sends password resets and invitations through the Mailgun HTTP messages API.

## Settings

Defined by `IPluginSettingsSchema` and stored under
`Settings:Plugins:mediapager.email.mailgun` (legacy `plugins.mailgun.*` values are migrated):

| Key | Label | Notes |
| --- | --- | --- |
| `apiKey` | Mailgun API key | required, secret |
| `domain` | Mailgun sending domain | required (e.g. `mg.yourdomain.com`) |
| `from` | From address | required |
| `apiBaseUrl` | Mailgun API base URL | defaults to `https://api.mailgun.net`, must be HTTPS |

Configure them in **Settings → General → Email provider → settings** in the SPA. Environment
fallbacks use `MEDIAPAGER_Settings__Plugins__mediapager.email.mailgun__<key>`. Values saved in
the UI are stored in the runtime database; all plugin setting values are protected at rest and
secret fields are never returned by the API. Your Mailgun domain needs to be verified (DNS
records) for delivery to work.
