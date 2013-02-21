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
    class ImgurAPI
    {
        public ImgurAPI()
        {
            

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
                        HttpResponseMessage response = await client.PostAsync("https://api.imgur.com/3/image", fullContent);
                        System.Diagnostics.Debug.WriteLine(await response.Content.ReadAsStringAsync());

                        Basic<UploadData> result = JSONHelper.Deserialize<Basic<UploadData>>(await response.Content.ReadAsStringAsync());
                        return result;

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
