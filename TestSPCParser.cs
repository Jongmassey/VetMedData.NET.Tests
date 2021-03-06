﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using VetMedData.NET.Model;
using VetMedData.NET.Util;

namespace VetMedData.Tests
{
    [TestClass]
    public class TestSPCParser
    {
        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestTargetSpeciesExtraction()
        {
            const string pathtospc = @"TestFiles\TestSPCParser\SPC_91079.docx";

            var expectedoutput = new[]
            {
                "horses", "ponies", "donkies", "foals over four weeks of age"
            };

            var ts = SPCParser.GetTargetSpecies(pathtospc);
            var intersectioncount = ts.Intersect(expectedoutput).Count();
            Assert.IsTrue(intersectioncount == expectedoutput.Length,
                $"Intersection count:{intersectioncount}, expected {expectedoutput.Length}");
        }

        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestTargetSpeciesExtractionFromDoc()
        {
            const string pathtospc = @"TestFiles\TestSPCParser\SPC_244480.doc";

            var expectedoutput = new[]
            {
                "horses", "dogs", "cats"
            };
            var pathtodocx = WordConverter.ConvertDocToDocx(pathtospc);
            var ts = SPCParser.GetTargetSpecies(pathtodocx);
            var intersectioncount = ts.Intersect(expectedoutput).Count();
            Assert.IsTrue(intersectioncount == expectedoutput.Length,
                $"Intersection count:{intersectioncount}, expected {expectedoutput.Length}");
        }
        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestTargetSpeciesExtractionFromDocOldFormat()
        {
            const string pathtospc = @"TestFiles\TestSPCParser\SPC_124816.doc";

            var expectedoutput = new[]
            {
                "cats"
            };
            var pathtodocx = WordConverter.ConvertDocToDocx(pathtospc);
            var ts = SPCParser.GetTargetSpecies(pathtodocx);
            var intersectioncount = ts.Intersect(expectedoutput).Count();
            Assert.IsTrue(intersectioncount == expectedoutput.Length,
                $"Intersection count:{intersectioncount}, expected {expectedoutput.Length}");
        }

        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestGetPdfPlainText()
        {
            const string pathtopdf = @"TestFiles\TestSPCParser\WC500067567.pdf";

            var pt = SPCParser.GetPlainText(pathtopdf);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pt), "Plain text null or whitespace");
        }

        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestGetTargetSpeciesFromPDF()
        {
            const string pathtopdf = @"TestFiles\TestSPCParser\WC500067567.pdf";
            var sp = SPCParser.GetTargetSpeciesFromPdf(pathtopdf);
            Assert.IsNotNull(sp, "Nothing returned");
            Assert.IsTrue(sp.Length == 1, $"Returned {sp.Length} species instead of 1");
            Assert.IsTrue(sp[0].Equals("horse", StringComparison.InvariantCultureIgnoreCase),
                $"returned: {sp[0]} instead of horse");
        }

        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestGetTargetSpeciesFromMultiPDF()
        {
            const string pathtopdf = @"TestFiles\TestSPCParser\WC500065777.pdf";
            var sp = SPCParser.GetTargetSpeciesFromMultiProductPdf(pathtopdf);
            Assert.IsNotNull(sp, "null dictionary returned");
            Assert.IsFalse(sp.Count == 0, "empty dictionary returned");
            Assert.IsTrue(sp.ContainsKey("Metacam 20 mg/ml solution for injection for cattle, pigs and horses"),
                "product not found");
            Assert.IsTrue(
                sp["Metacam 20 mg/ml solution for injection for cattle, pigs and horses"]
                    .Intersect(new[] { "cattle", "pigs", "horses" }).Count() == 3,
                $"Unexpected species list returned:{string.Join(',', sp["Metacam 20 mg/ml solution for injection for cattle, pigs and horses"])}");
        }

        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestDicuralParsing()
        {
            const string pathToPdf = @"TestFiles\TestSPCParser\WC500062810.pdf";
            var ts = SPCParser.GetTargetSpeciesFromMultiProductPdf(pathToPdf);
            Assert.IsTrue(ts.Keys.Count == 6, $"6 product names should be returned: {ts.Keys.Count}");
            Assert.IsTrue(ts.All(k => k.Value != null && k.Value.Length > 0), "Empty target species array returned");
            Assert.IsFalse(ts.Values.SelectMany(v => v).Any(string.IsNullOrWhiteSpace), "Blank target species returned");
            Assert.IsTrue(ts.Where(kv => kv.Key.Contains("and")).All(kv => kv.Value.Length > 1), "multi-species product has single species");
        }

        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestIbaflinParsing()
        {
            const string pathToPdf = @"TestFiles\TestSPCParser\WC500064198.pdf";
            var ts = SPCParser.GetTargetSpeciesFromMultiProductPdf(pathToPdf);
            Assert.IsTrue(ts.Keys.Count==6,$"6 product names should be returned: {ts.Keys.Count}");
            Assert.IsTrue(ts.All(k=>k.Value != null && k.Value.Length>0),"Empty target species array returned");
            Assert.IsFalse(ts.Values.SelectMany(v=>v).Any(string.IsNullOrWhiteSpace),"Blank target species returned");
            Assert.IsTrue(ts.Where(kv => kv.Key.Contains("and")).All(kv => kv.Value.Length > 1), "multi-species product has single species");
        }

        [TestMethod, DeploymentItem(@"TestFiles\TestSPCParser\", @"TestFiles\TestSPCParser\")]
        public void TestBtvPurParsing()
        {
            const string pathtopdf = @"TestFiles\TestSPCParser\btvpur-alsap-8-epar-product-information_en.pdf";
            var sp = SPCParser.GetTargetSpeciesFromPdf(pathtopdf);
            Assert.IsNotNull(sp, "Nothing returned");
            Assert.IsTrue(sp.Length == 2, $"Returned {sp.Length} species instead of 2");
            Assert.IsTrue(sp.Intersect(new[] {"sheep", "cattle"}).Count() == 2,$"Returned {string.Join(',',sp)} instead of cattle,sheep");
        }

        [TestMethod]
        public void TestBTVpur()
        {
            var ep = new ExpiredProduct
            {
                Name = "btvpur alsap 8, suspension for injection",
                SPC_Link = "ema.europa.eu"

            };
            var spc = VMDPIDFactory.GetSpc(ep).Result;
            var sp = SPCParser.GetTargetSpeciesFromPdf(spc);
            Assert.IsNotNull(sp, "Nothing returned");
            Assert.IsTrue(sp.Length == 2, $"Returned {sp.Length} species instead of 2");
            Assert.IsTrue(sp.Intersect(new[] { "sheep", "cattle" }).Count() == 2, $"Returned {string.Join(',', sp)} instead of cattle,sheep");
        }

        [TestMethod]
        public void TestPurevaxRcch()
        {
            var ep = new ExpiredProduct
            {
                Name = "purevax rcch",
                SPC_Link = "ema.europa.eu"

            };
            var spc = VMDPIDFactory.GetSpc(ep).Result;
            var sp = SPCParser.GetTargetSpeciesFromPdf(spc);
            Assert.IsNotNull(sp, "Nothing returned");
            Assert.IsTrue(sp.Length == 1, $"Returned {sp.Length} species instead of 1");
            Assert.IsTrue(sp.Intersect(new[] { "cats"}).Count() == 1, $"Returned {string.Join(',', sp)} instead of cats");
        }
    }
}
