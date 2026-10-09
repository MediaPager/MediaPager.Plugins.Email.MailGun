# MediaPager.Plugins.Email.MailGun

Official Mailgun email provider plugin for [MediaPager](https://github.com/MediaPager/MediaPager).
Ships with the app and is **loaded by default**.

Sends password resets and invitations through the Mailgun HTTP messages API.

## Settings

Defined by `IPluginSettingsSchema` and stored under `plugins.mailgun.*`:

| Key | Label | Notes |
| --- | --- | --- |
| `apiKey` | Mailgun API key | required, secret |
| `domain` | Mailgun sending domain | required (e.g. `mg.yourdomain.com`) |
| `from` | From address | required |
| `apiBaseUrl` | Mailgun API base URL | defaults to `https://api.mailgun.net`, must be HTTPS |

Configure them in **Settings → Email** in the SPA. Your Mailgun domain needs to be
verified (DNS records) for delivery to work.
