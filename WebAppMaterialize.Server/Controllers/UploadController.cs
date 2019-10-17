
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DTHelperStd;
using Microsoft.Extensions.Configuration;

namespace WebAppMaterialize.Server.Controllers
{
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private const int SAMPLE_LINE_NUMBER = 10;

        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        private readonly IConfiguration Configuration;

        public UploadController(IConfiguration config)
        {
            this.Configuration = config;
        }
        private FileMessage FileMessage { get; set; } = new FileMessage();

        [HttpPost]
        public async Task<FileMessage> Upload()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            var tempDir = this.Configuration.GetSection("FileSettings").GetSection("filePath").Value;
            //Create a temp directory in UserData
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // Used to accumulate all the form url encoded key value pairs in the 
            // request.
            var formAccumulator = new KeyValueAccumulator();
            string targetFilePath = null;

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        targetFilePath = Path.Combine(tempDir, $"temp_{ DateTime.Today.ToShortDateString()}");
                        using (var targetStream = System.IO.File.Create(targetFilePath))
                        {
                            await section.Body.CopyToAsync(targetStream);
                        }
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        // Content-Disposition: form-data; name="key"
                        //
                        // value

                        // Do not limit the key name length here because the 
                        // multipart headers length limit is already in effect.
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024 * 100,
                            leaveOpen: true))
                        {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            var value = await streamReader.ReadToEndAsync();
                            if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = String.Empty;
                            }
                            formAccumulator.Append(key.Value, value);

                            if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
            var fileSample = string.Empty;
            using (var fileReader = new StreamReader(targetFilePath))
            {
                string newLine = string.Empty;
                var lineNumber = 0;
                while ((newLine = fileReader.ReadLine()) != null && lineNumber < SAMPLE_LINE_NUMBER)
                {
                    fileSample += newLine + "\n";
                    lineNumber++;
                }
            }
            FileMessage = new FileMessage
            {
                FilePath = targetFilePath,
                FileSample = fileSample
            };
            return FileMessage;
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }
    }

    public class FileMessage
    {
        public string FileSample { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}

