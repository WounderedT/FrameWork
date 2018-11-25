using Downloader;
using Downloader.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DownloaderUnitTests
{
    [TestClass]
    public class PatternEntryViewModelUnitTests
    {
        [DataRow(
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://c.xme.net/c255348.jpg",
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg")]
        [DataRow(
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg")]
        [DataRow(
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://c.xme.net/new/sub/link/c255348.jpg",
            "https://c.xme.net/#@#_INDEXER_#@#new/sub/link/c255348.jpg")]
        [DataRow(
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://brand.new.link.com/pic#@#_INDEXER_#@#.jpg",
            "https://brand.new.link.com/pic#@#_INDEXER_#@#.jpg")]
        [DataRow(
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://c.xme.net/#@#_INDEXER_#@#c2#@#_KEY1_#@#.jpg",
            "https://c.xme.net/#@#_INDEXER_#@#c2#@#_KEY1_#@#.jpg")]
        [DataRow(
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://c.xme.net/c2#@#_KEY1_#@#.jpg",
            "https://c.xme.net/#@#_INDEXER_#@#c2#@#_KEY1_#@#.jpg")]
        [DataRow(
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://c.xme.net_new/sub/link_c2#@#_KEY1_#@#.jpg",
            "https://c.xme.net_new/sub/link_c2#@#_KEY1_#@#.jpg")]
        [DataRow(
            "https://c.xme.net/#@#_INDEXER_#@#c255348.jpg",
            "https://brand.new/#@#_INDEXER_#@#/link.net/#@#_INDEXER_#@#pic.jpg",
            "https://brand.new/#@#_INDEXER_#@#/link.net/#@#_INDEXER_#@#pic.jpg")]
        [DataRow(
            "https://brand.new/#@#_INDEXER_#@#/link.net/#@#_INDEXER_#@#pic.jpg",
            "https://brand.new//link.com/pic#@#_KEY1_#@#.jpg",
            "https://brand.new/#@#_INDEXER_#@#/link.com/#@#_INDEXER_#@#pic#@#_KEY1_#@#.jpg")]
        [DataRow(
            "https://brand.new/link.net/pic#@#_INDEXER_#@##@#_INDEXER_#@#.jpg",
            "https://brand.new/link.net/pic.jpg",
            "https://brand.new/link.net/pic#@#_INDEXER_#@##@#_INDEXER_#@#.jpg")]
        [DataRow(
            "https://c.xme.net/c255348#@#_INDEXER_#@#.jpg",
            "https://c.xme.net/c#@#_INDEXER_#@#.jpg",
            "https://c.xme.net/c#@#_INDEXER_#@#.jpg")]
        [DataRow(
            "https://c.xme.net/c255348#@#_INDEXER_#@#.jpg",
            "https://c.xme.net/c#@#_INDEXER_#@#255348.jpg",
            "https://c.xme.net/c#@#_INDEXER_#@#255348.jpg")]
        [DataTestMethod]
        public void MergeLinkTest(string internalLink, string newLink, string result)
        {
            var privateObj = new PrivateObject(typeof(PatternEntryViewModel), new[] { typeof(Pattern) }, new object []{ new Pattern() });
            privateObj.SetField("_patternDownloadLinkInternal", internalLink);
            privateObj.SetField("_patternDownloadLink", newLink);

            privateObj.Invoke("MergeLinks");

            Assert.AreEqual(result, privateObj.GetField("_patternDownloadLinkInternal"));
        }
    }
}
