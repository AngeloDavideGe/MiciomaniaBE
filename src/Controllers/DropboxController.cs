using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Miciomania.Views.DropboxSettings;


[ApiController]
[Route("api/dropbox")]
public class DropboxController : ControllerBase
{
    private readonly DropboxSettings _dropboxSettings;
    private readonly IHttpClientFactory _httpClientFactory;

    public DropboxController(IOptions<DropboxSettings> options, IHttpClientFactory httpClientFactory)
    {
        _dropboxSettings = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("get_access_token")]
    public async Task<IActionResult> GetAccessToken()
    {
        HttpClient client = _httpClientFactory.CreateClient();

        FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", _dropboxSettings.RefreshToken },
            { "client_id", _dropboxSettings.ClientId },
            { "client_secret", _dropboxSettings.ClientSecret }
        });

        HttpResponseMessage response = await client.PostAsync("https://api.dropboxapi.com/oauth2/token", content);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, error);
        }

        string responseContent = await response.Content.ReadAsStringAsync();
        return Ok(responseContent); // Restituisce JSON con access_token ecc.
    }
}
