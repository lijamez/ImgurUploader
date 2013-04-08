using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ImgurUploader
{
    /// <summary>
    /// https://api.imgur.com/endpoints
    /// </summary>
    class ImgurAPI
    {
        public ImgurAPI()
        {

        }

        private async Task RefreshAccessToken()
        {
            ImgurHttpClient client = ImgurHttpClient.Instance;

            try
            {

                using (MultipartFormDataContent fullContent = new MultipartFormDataContent())
                {
                    fullContent.Add(new StringContent(client.LogInState.RefreshToken), "refresh_token");
                    fullContent.Add(new StringContent(client.ClientID), "client_id");
                    fullContent.Add(new StringContent(client.ClientSecret), "client_secret");
                    fullContent.Add(new StringContent("refresh_token"), "grant_type");

                    System.Diagnostics.Debug.WriteLine("Getting a new access token...");
                    using (HttpResponseMessage response = await client.Client.PostAsync("https://api.imgur.com/oauth2/token", fullContent))
                    {
                        RefreshedAccessToken result = JSONHelper.Deserialize<RefreshedAccessToken>(await response.Content.ReadAsStringAsync());

                        if (result != null)
                        {
                            client.LogIn(result.AccessToken, result.TokenType, DateTime.UtcNow.AddSeconds(result.ExpiresIn), result.RefreshToken, client.LogInState.AccountUsername);
                            System.Diagnostics.Debug.WriteLine("Successfully received new access token.");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to get new access token.");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                client.LogOut();
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private async Task<HttpClient> GetImgurHttpClient()
        {
            if (ImgurHttpClient.Instance.LogInState.LoggedIn && !ImgurHttpClient.Instance.TokensValid())
            {
                await RefreshAccessToken();
            }

            if (String.IsNullOrEmpty(ImgurHttpClient.Instance.ClientID))
            {
                await ImgurHttpClient.Instance.ReadAPIKeys();
            }

            return ImgurHttpClient.Instance.Client;
        }



        /// <summary>
        /// For some reason, imgur requires a cookie to create albums. If there is no cookie, then this will fail with a 405 error. (Imgur didn't document this!!!)
        /// Simply upload some images prior to calling this method.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="cover"></param>
        /// <returns></returns>
        public async Task<Basic<AlbumCreateData>> CreateAlbum(string[] ids, string title, string description, string cover)
        {
            try
            {
                HttpClient client = await GetImgurHttpClient();

                using (MultipartFormDataContent fullContent = new MultipartFormDataContent())
                {
                    if (ids != null)
                    {
                        foreach (string id in ids)
                        {
                            fullContent.Add(new StringContent(id), "ids[]");
                        }
                    }

                    if (!String.IsNullOrEmpty(title))
                        fullContent.Add(new StringContent(title), "title");
                    if (!String.IsNullOrEmpty(description))
                        fullContent.Add(new StringContent(description), "description");
                    if (!String.IsNullOrEmpty(cover))
                        fullContent.Add(new StringContent(cover), "cover");

                    System.Diagnostics.Debug.WriteLine("Creating album...");
                    using (HttpResponseMessage response = await client.PostAsync("https://api.imgur.com/3/album", fullContent))
                    {
                        System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());

                        Basic<AlbumCreateData> result = JSONHelper.Deserialize<Basic<AlbumCreateData>>(await response.Content.ReadAsStringAsync());
                        return result;
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
        }


        public async Task<Basic<UploadData>> Upload(Stream imageStream, string fileName, string title, string description, string albumId) 
        {
            try
            {
                HttpClient client = await GetImgurHttpClient();

                using (HttpContent imageContent = new StreamContent(imageStream))
                {
                    using (MultipartFormDataContent fullContent = new MultipartFormDataContent())
                    {

                        fullContent.Add(imageContent, "image");
                        if (!String.IsNullOrEmpty(fileName))
                            fullContent.Add(new StringContent(fileName), "name");
                        if (!String.IsNullOrEmpty(title))
                            fullContent.Add(new StringContent(title), "title");
                        if (!String.IsNullOrEmpty(albumId))
                            fullContent.Add(new StringContent(albumId), "album_id");
                        if (!String.IsNullOrEmpty(description))
                            fullContent.Add(new StringContent(description), "description");

                        System.Diagnostics.Debug.WriteLine(String.Format("Uploading file '{0}'...", fileName));
                        using (HttpResponseMessage response = await client.PostAsync("https://api.imgur.com/3/image", fullContent))
                        {
                            System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());

                            Basic<UploadData> result = JSONHelper.Deserialize<Basic<UploadData>>(await response.Content.ReadAsStringAsync());
                            return result;
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }
            
        }

        public async Task<Basic<UploadData>> Upload(StorageFile file, string title, string description, string albumId)
        {
            using (Stream imageStream = await WindowsRuntimeStorageExtensions.OpenStreamForReadAsync(file))
            {
                return await Upload(imageStream, file.Name, title, description, null);
            }
        }

    }
}
