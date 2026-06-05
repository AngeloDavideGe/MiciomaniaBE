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
    [FromForm] string folderPath)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient();

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

            string extension =
                Path.GetExtension(file.FileName);

            string fileName =
                Path.GetFileNameWithoutExtension(folderPath) + extension;

            string dropboxPath =
                $"/Utenti/{userId}/{fileName}";

            byte[] fileBytes;

            using (MemoryStream ms = new())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            HttpRequestMessage uploadRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://content.dropboxapi.com/2/files/upload"
                );

            uploadRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken
                );

            uploadRequest.Headers.Add(
                "Dropbox-API-Arg",
                JsonSerializer.Serialize(new
                {
                    path = dropboxPath,
                    mode = "overwrite",
                    autorename = false,
                    mute = false
                }));

            ByteArrayContent fileContent =
                new ByteArrayContent(fileBytes);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            uploadRequest.Content = fileContent;

            HttpResponseMessage uploadResponse =
                await client.SendAsync(uploadRequest);

            if (!uploadResponse.IsSuccessStatusCode)
            {
                string uploadError =
                    await uploadResponse.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)uploadResponse.StatusCode,
                    uploadError);
            }

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
                    "application/json"
                );

            HttpResponseMessage shareResponse =
                await client.SendAsync(shareRequest);

            string shareJson =
                await shareResponse.Content.ReadAsStringAsync();

            if (!shareResponse.IsSuccessStatusCode)
            {
                // Se il link condiviso esiste già
                if (shareJson.Contains("shared_link_already_exists"))
                {
                    HttpRequestMessage existingLinkRequest =
                        new HttpRequestMessage(
                            HttpMethod.Post,
                            "https://api.dropboxapi.com/2/sharing/list_shared_links"
                        );

                    existingLinkRequest.Headers.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            accessToken
                        );

                    existingLinkRequest.Content =
                        new StringContent(
                            JsonSerializer.Serialize(new
                            {
                                path = dropboxPath,
                                direct_only = true
                            }),
                            Encoding.UTF8,
                            "application/json"
                        );

                    HttpResponseMessage existingLinkResponse =
                        await client.SendAsync(existingLinkRequest);

                    string existingLinkJson =
                        await existingLinkResponse.Content.ReadAsStringAsync();

                    if (!existingLinkResponse.IsSuccessStatusCode)
                    {
                        return StatusCode(
                            (int)existingLinkResponse.StatusCode,
                            existingLinkJson
                        );
                    }

                    using JsonDocument existingDoc =
                        JsonDocument.Parse(existingLinkJson);

                    string existingUrl =
                        existingDoc.RootElement
                            .GetProperty("links")[0]
                            .GetProperty("url")
                            .GetString()!;

                    string existingDirectUrl =
                        existingUrl
                            .Replace("www.dropbox.com", "dl.dropboxusercontent.com")
                            .Replace("?dl=0", "");

                    return Ok(new
                    {
                        url = existingDirectUrl
                    });
                }

                return StatusCode(
                    (int)shareResponse.StatusCode,
                    shareJson
                );
            }

            using JsonDocument shareDoc =
                JsonDocument.Parse(shareJson);

            string sharedUrl =
                shareDoc.RootElement
                    .GetProperty("url")
                    .GetString()!;

            string directUrl =
                sharedUrl
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
