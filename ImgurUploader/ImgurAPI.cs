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
                HttpClient client = ImgurHttpClient.Instance;


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
                HttpClient client = ImgurHttpClient.Instance;

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
