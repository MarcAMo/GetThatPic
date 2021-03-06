// <copyright file="HttpRequesterTests.cs" company="Marc A. Modrow">
// Copyright (c) 2018 All Rights Reserved
// <author>Marc A. Modrow</author>
// </copyright>
using System;
using System.IO;
using System.Threading.Tasks;
using GetThatPic.Data.IO;
using Xunit;

namespace GetThatPic.Test.Data.IO
{
    /// <summary>
    /// Tests the HttpRequester Functionality.
    /// </summary>
    public class HttpRequesterTests
    {
        /// <summary>
        /// Gets a null url.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetString_NullAsync()
        {
            Assert.Null(await HttpRequester.GetString(null));
        }

        /// <summary>
        /// Gets an empty url.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetString_Empty()
        {
            Assert.Null(await HttpRequester.GetString(string.Empty));
        }

        /// <summary>
        /// Gets a non url.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetString_NonUrl()
        {
            Assert.Null(await HttpRequester.GetString("this is no path"));
        }

        /// <summary>
        /// Gets the google.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetString_Google()
        {
            string response = await HttpRequester.GetString("https://google.com/");
            Assert.Contains("<title>Google</title>", response);
        }

        /// <summary>
        /// Gets an non-existing url.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetString_404()
        {
            string response = await HttpRequester.GetString("https://sgosdbodgosdgosdgosdgsg.gosgsdgogle.cokl�sdgkl�jm/");
            Assert.Null(response);
        }

        /// <summary>
        /// Gets a null url.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetStream_NullAsync()
        {
            Assert.Null(await HttpRequester.GetStream(null));
        }

        /// <summary>
        /// Gets an empty url.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetStream_Empty()
        {
            Assert.Null(await HttpRequester.GetStream(string.Empty));
        }

        /// <summary>
        /// Gets a non url.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetStream_NonUrl()
        {
            Assert.Null(await HttpRequester.GetStream("this is no path"));
        }

        /// <summary>
        /// Gets the google.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetStream_Google()
        {
            Stream stream = await HttpRequester.GetStream("https://google.com/");
            StreamReader reader = new StreamReader(stream);
            Assert.Contains("<title>Google</title>", reader.ReadToEnd());
        }

        /// <summary>
        /// Gets an non-existing url.
        /// </summary>
        /// <returns>A Task for async.</returns>
        [Fact]
        public async Task GetStream_404()
        {
            Stream response = await HttpRequester.GetStream("https://sgosdbodgosdgosdgosdgsg.gosgsdgogle.cokl�sdgkl�jm/");
            Assert.Null(response);
        }
    }
}
