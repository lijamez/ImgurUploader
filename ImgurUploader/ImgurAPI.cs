using ImgurUploader.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
        public const string ALBUM_PRIVACY_PUBLIC = "public";
        public const string ALBUM_PRIVACY_HIDDEN = "hidden";
        public const string ALBUM_PRIVACY_SECRET = "secret";

        public const string ALBUM_LAYOUT_BLOG = "blog";
        public const string ALBUM_LAYOUT_GRID = "grid";
        public const string ALBUM_LAYOUT_HORIZONTAL = "horizontal";
        public const string ALBUM_LAYOUT_VERTICAL = "vertical";

        public ImgurAPI()
        {

        }

        private async Task RefreshAccessToken(CancellationToken cancelToken)
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
                    using (HttpResponseMessage response = await client.Client.PostAsync("https://api.imgur.com/oauth2/token", fullContent, cancelToken))
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
            catch (IOException ex)
            {
                client.LogOut();
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private async Task<HttpClient> GetImgurHttpClient(CancellationToken cancelToken)
        {
            if (ImgurHttpClient.Instance.LogInState.CredentialsDefined && !ImgurHttpClient.Instance.TokensValid())
            {
                System.Diagnostics.Debug.WriteLine("Token has been invalidated/expired. Trying to get a new one...");
                await RefreshAccessToken(cancelToken);
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
        public async Task<Basic<AlbumCreateData>> CreateAlbum(string[] ids, string title, string description, string cover, CancellationToken cancelToken)
        {
            try
            {
                HttpClient client = await GetImgurHttpClient(cancelToken);

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
                    using (HttpResponseMessage response = await client.PostAsync("https://api.imgur.com/3/album", fullContent, cancelToken))
                    {
                        System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());

                        Basic<AlbumCreateData> result = JSONHelper.Deserialize<Basic<AlbumCreateData>>(await response.Content.ReadAsStringAsync());
                        return result;
                    }
                }

            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return null;
        }

        public async Task<Basic<Boolean>> UpdateAlbum(string albumId, string[] ids, string title, string description, string privacy, string layout, string cover, CancellationToken cancelToken)
        {
            try
            {
                HttpClient client = await GetImgurHttpClient(cancelToken);

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
                    if (!String.IsNullOrEmpty(privacy))
                        fullContent.Add(new StringContent(privacy), "privacy");
                    if (!String.IsNullOrEmpty(layout))
                        fullContent.Add(new StringContent(layout), "layout");
                    if (!String.IsNullOrEmpty(cover))
                        fullContent.Add(new StringContent(cover), "cover");

                    System.Diagnostics.Debug.WriteLine("Updating album...");
                    using (HttpResponseMessage response = await client.PostAsync(String.Format("https://api.imgur.com/3/album/{0}", albumId), fullContent, cancelToken))
                    {
                        System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());

                        Basic<Boolean> result = JSONHelper.Deserialize<Basic<Boolean>>(await response.Content.ReadAsStringAsync());
                        return result;
                    }
                }
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return null;
        }

        public async Task<Basic<Boolean>> DeleteAlbum(string deleteHash, CancellationToken cancelToken)
        {
            try
            {
                HttpClient client = await GetImgurHttpClient(cancelToken);

                using (MultipartFormDataContent fullContent = new MultipartFormDataContent())
                {
                    System.Diagnostics.Debug.WriteLine("Deleting album...");
                    using (HttpResponseMessage response = await client.DeleteAsync(String.Format("https://api.imgur.com/3/album/{0}", deleteHash), cancelToken))
                    {
                        System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());

                        Basic<Boolean> result = JSONHelper.Deserialize<Basic<Boolean>>(await response.Content.ReadAsStringAsync());
                        return result;
                    }
                }
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return null;
        }

        public async Task<Basic<UploadData>> Upload(Stream imageStream, string fileName, string title, string description, string albumId, CancellationToken cancelToken) 
        {
            try
            {
                HttpClient client = await GetImgurHttpClient(cancelToken);

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
                            using (HttpResponseMessage response = await client.PostAsync("https://api.imgur.com/3/image", fullContent, cancelToken))
                            {
                                System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());

                                Basic<UploadData> result = JSONHelper.Deserialize<Basic<UploadData>>(await response.Content.ReadAsStringAsync());
                                return result;
                            }
                    }
                }
                
            }
            catch (IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

            return null;
        }

        public async Task<Basic<UploadData>> Upload(StorageFile file, string title, string description, string albumId, CancellationToken cancelToken)
        {
            using (Stream imageStream = await WindowsRuntimeStorageExtensions.OpenStreamForReadAsync(file))
            {
                return await Upload(imageStream, file.Name, title, description, null, cancelToken);
            }
        }

        public async Task<Basic<Boolean>> DeleteImage(string deleteHash, CancellationToken cancelToken)
        {
            try
            {
                HttpClient client = await GetImgurHttpClient(cancelToken);

                using (MultipartFormDataContent fullContent = new MultipartFormDataContent())
                {
                    System.Diagnostics.Debug.WriteLine("Deleting image...");
                    using (HttpResponseMessage response = await client.DeleteAsync(String.Format("https://api.imgur.com/3/image/{0}", deleteHash), cancelToken))
                    {
                        System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());

                        Basic<Boolean> result = JSONHelper.Deserialize<Basic<Boolean>>(await response.Content.ReadAsStringAsync());
                        return result;
                    }
                }
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return null;
        }
    }
}
