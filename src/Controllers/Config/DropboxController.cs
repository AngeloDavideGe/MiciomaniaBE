using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Views.DropboxSettings;

[ApiController]
[Route("api/dropbox")]
public class DropboxController : ControllerBase
{
    private readonly DropboxSettings _dropboxSettings;
    private readonly IHttpClientFactory _httpClientFactory;

    public DropboxController(
        IOptions<DropboxSettings> options,
        IHttpClientFactory httpClientFactory)
    {
        _dropboxSettings = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<UploadResponse>> Upload(
        IFormFile file,
        [FromForm] string userId,
        [FromForm] string folderPath,
        [FromForm] string? oldLink)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient();

            // =========================
            // 1. GET ACCESS TOKEN
            // =========================

            FormUrlEncodedContent tokenContent =
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", _dropboxSettings.RefreshToken },
                    { "client_id", _dropboxSettings.ClientId },
                    { "client_secret", _dropboxSettings.ClientSecret }
                });

            HttpResponseMessage tokenResponse =
                await client.PostAsync(
                    "https://api.dropboxapi.com/oauth2/token",
                    tokenContent);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                string tokenError =
                    await tokenResponse.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)tokenResponse.StatusCode,
                    tokenError);
            }

            string tokenJson =
                await tokenResponse.Content.ReadAsStringAsync();

            using JsonDocument tokenDoc =
                JsonDocument.Parse(tokenJson);

            string accessToken =
                tokenDoc.RootElement
                    .GetProperty("access_token")
                    .GetString()!;

            // =========================
            // 2. UPLOAD FILE
            // =========================

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"FileUtente{extension}";
            string dropboxPath = $"/{folderPath}/{userId}/{fileName}";

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken
                );

            client.DefaultRequestHeaders.Add(
                "Dropbox-API-Arg",
                JsonSerializer.Serialize(new
                {
                    path = dropboxPath,
                    mode = "overwrite",
                    autorename = false,
                    mute = false
                }));

            byte[] fileBytes;

            using (MemoryStream ms = new())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            ByteArrayContent fileContent =
                new ByteArrayContent(fileBytes);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage uploadResponse =
                await client.PostAsync(
                    "https://content.dropboxapi.com/2/files/upload",
                    fileContent
                );

            if (!uploadResponse.IsSuccessStatusCode)
            {
                string uploadError =
                    await uploadResponse.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)uploadResponse.StatusCode,
                    uploadError);
            }

            // =========================
            // 3. RETURN OLD LINK
            // =========================

            if (!string.IsNullOrWhiteSpace(oldLink))
            {
                return Ok(new
                {
                    url = oldLink
                });
            }

            // =========================
            // 4. CREATE SHARED LINK
            // =========================

            HttpRequestMessage shareRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.dropboxapi.com/2/sharing/create_shared_link_with_settings"
                );

            shareRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken
                );

            shareRequest.Content =
                new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        path = dropboxPath,
                        settings = new
                        {
                            requested_visibility = "public"
                        }
                    }),
                    Encoding.UTF8,
                    "application/json");

            HttpResponseMessage shareResponse =
                await client.SendAsync(shareRequest);

            string shareJson = await shareResponse.Content.ReadAsStringAsync();

            if (!shareResponse.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)shareResponse.StatusCode,
                    shareJson
                );
            }

            using JsonDocument shareDoc = JsonDocument.Parse(shareJson);

            string sharedUrl = shareDoc.RootElement
                .GetProperty("url")
                .GetString()!;

            // =========================
            // 5. DIRECT LINK
            // =========================

            string directUrl = sharedUrl
                .Replace("www.dropbox.com", "dl.dropboxusercontent.com")
                .Replace("?dl=0", "");

            return Ok(new
            {
                url = directUrl
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
