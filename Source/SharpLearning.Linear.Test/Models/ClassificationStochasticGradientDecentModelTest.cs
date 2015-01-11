﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpLearning.Containers;
using SharpLearning.InputOutput.Csv;
using SharpLearning.Linear.Learners;
using SharpLearning.Linear.Models;
using SharpLearning.Linear.Test.Properties;
using SharpLearning.Metrics.Classification;
using SharpLearning.Metrics.Regression;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SharpLearning.Linear.Test.Models
{
    [TestClass]
    public class ClassificationStochasticGradientDecentModelTest
    {
        [TestMethod]
        public void ClassificationStochasticGradientDecentModel_Predict_Single()
        {
            var parser = new CsvParser(() => new StringReader(Resources.LogitTest));
            var observations = parser.EnumerateRows("F1", "F2").ToF64Matrix();
            var targets = parser.EnumerateRows("Target").ToF64Vector();

            var learner = new ClassificationStochasticGradientDecentLearner(0.0001, 100000, 0.0, 42, 1);
            var sut = learner.Learn(observations, targets);

            var predictions = new double[targets.Length];
            for (int i = 0; i < predictions.Length; i++)
            {
                predictions[i] = sut.Predict(observations.GetRow(i));
            }

            var metric = new TotalErrorClassificationMetric<double>();
            var actual = metric.Error(targets, predictions);
            Assert.AreEqual(0.11, actual, 0.001);
        }

        [TestMethod]
        public void ClassificationStochasticGradientDecentModel_Predict_Multi()
        {
            var parser = new CsvParser(() => new StringReader(Resources.LogitTest));
            var observations = parser.EnumerateRows("F1", "F2").ToF64Matrix();
            var targets = parser.EnumerateRows("Target").ToF64Vector();

            var sut = new ClassificationStochasticGradientDecentLearner(0.0001, 100000, 0.0, 42, 1);
            var model = sut.Learn(observations, targets);

            var metric = new TotalErrorClassificationMetric<double>();

            var predictions = model.Predict(observations);
            var actual = metric.Error(targets, predictions);
            Assert.AreEqual(0.11, actual, 0.001);
        }

        [TestMethod]
        public void ClassificationStochasticGradientDecentModel_GetRawVariableImportance()
        {
            var parser = new CsvParser(() => new StringReader(Resources.LogitTest));
            var observations = parser.EnumerateRows("F1", "F2").ToF64Matrix();
            var targets = parser.EnumerateRows("Target").ToF64Vector();

            var sut = new ClassificationStochasticGradientDecentLearner(0.0001, 100000, 0.0, 42, 1);
            var model = sut.Learn(observations, targets);

            var actual = model.GetRawVariableImportance();
            Assert.AreEqual(0.14011316001536858, actual[0], 0.001);
            Assert.AreEqual(0.13571128043372779, actual[1], 0.001);
        }

        [TestMethod]
        public void ClassificationStochasticGradientDecentModel_GetVariableImportance()
        {
            var parser = new CsvParser(() => new StringReader(Resources.LogitTest));
            var observations = parser.EnumerateRows("F1", "F2").ToF64Matrix();
            var featureNameToIndex = parser.EnumerateRows("F1", "F2").First().ColumnNameToIndex;
            var targets = parser.EnumerateRows("Target").ToF64Vector();

            var sut = new ClassificationStochasticGradientDecentLearner(0.0001, 100000, 0.0, 42, 1);
            var model = sut.Learn(observations, targets);

            var actual = model.GetVariableImportance(featureNameToIndex).ToList();
            var expected = new Dictionary<string, double> { { "F1", 100.0 }, {"F2", 96.8583396583461} }.ToList();
            
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Key, actual[i].Key);
                Assert.AreEqual(expected[i].Value, actual[i].Value, 0.0001);
            }
        }

        [TestMethod]
        public void ClassificationStochasticGradientDecentModel_PredictProbability_Single()
        {
            var parser = new CsvParser(() => new StringReader(Resources.LogitTest));
            var observations = parser.EnumerateRows("F1", "F2").ToF64Matrix();
            var featureNameToIndex = parser.EnumerateRows("F1", "F2").First().ColumnNameToIndex;
            var targets = parser.EnumerateRows("Target").ToF64Vector();
            var rows = targets.Length;

            var learner = new ClassificationStochasticGradientDecentLearner(0.0001, 100000, 0.0, 42, 1);
            var sut = learner.Learn(observations, targets);

            var actual = new ProbabilityPrediction[rows];
            for (int i = 0; i < rows; i++)
            {
                actual[i] = sut.PredictProbability(observations.GetRow(i));
            }

            var evaluator = new TotalErrorClassificationMetric<double>();
            var error = evaluator.Error(targets, actual.Select(p => p.Prediction).ToArray());

            Assert.AreEqual(0.11, error, 0.0000001);
            var expected = new ProbabilityPrediction[] { new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.621101688132576 }, { 1, 0.0846881846950576 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.996775267107844 }, { 1, 0.000978576747651287 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.734571906910202 }, { 1, 0.0560691486719206 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0146128977972945 }, { 1, 0.864567303084922 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00465883607592212 }, { 1, 0.951584191188203 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.878121560867104 }, { 1, 0.0269925976131346 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00325193200227069 }, { 1, 0.959527740814359 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.289961572914445 }, { 1, 0.288441954290256 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00136926544946117 }, { 1, 0.982308673748164 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.141385058527512 }, { 1, 0.486304759053574 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0638775362194203 }, { 1, 0.686490500097714 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.780859048908629 }, { 1, 0.0589063271468531 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00252880042425468 }, { 1, 0.971519850769858 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00087001279403044 }, { 1, 0.98740619000205 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.518839497800345 }, { 1, 0.120766688297536 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0233345267349947 }, { 1, 0.802122627607638 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.288881083312414 }, { 1, 0.280995279161987 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.519797291829791 }, { 1, 0.142775277629942 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00138927920018171 }, { 1, 0.981417425379206 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.212813784372327 }, { 1, 0.369635773422515 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.663944289044126 }, { 1, 0.0898669644240356 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00383278151966456 }, { 1, 0.961609129679238 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.902332538939807 }, { 1, 0.0227177491768366 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.994182189991355 }, { 1, 0.0016750314168006 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0128151441950428 }, { 1, 0.890023451669696 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0924308234966023 }, { 1, 0.551463210142294 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.200333386383766 }, { 1, 0.390327696898008 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0848695208602149 }, { 1, 0.622509079619917 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.610815632228507 }, { 1, 0.103863511345075 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.842803823944141 }, { 1, 0.0331355416443153 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0726545903870118 }, { 1, 0.605067021309892 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0224314022017193 }, { 1, 0.839079865094142 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.516529956095135 }, { 1, 0.131192031376301 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.314151760775334 }, { 1, 0.238567128919405 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.654369918912128 }, { 1, 0.0782914222546364 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.767308909284225 }, { 1, 0.0549450288457183 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0966147800476709 }, { 1, 0.492180181887752 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0173290689348948 }, { 1, 0.849659037294936 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.453472079161749 }, { 1, 0.180062953612024 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.717906972566941 }, { 1, 0.059376160140945 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0303782758955656 }, { 1, 0.797779204189589 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.913160268853319 }, { 1, 0.0205073104224625 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00203157004984982 }, { 1, 0.97817527074423 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.246194313426826 }, { 1, 0.340831406377204 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.928453252401366 }, { 1, 0.0171059670547643 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.537630876318061 }, { 1, 0.13082767370801 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0116187961764023 }, { 1, 0.897660135429518 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 7.22906642570243E-05 }, { 1, 0.998775474649305 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00274401349818616 }, { 1, 0.965087830900596 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000132042182498753 }, { 1, 0.997853116657023 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00481206796842223 }, { 1, 0.950572498548318 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0019202922181901 }, { 1, 0.979858947815663 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0665401709392385 }, { 1, 0.670270309873586 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.947870386295566 }, { 1, 0.0114755278656634 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.893732847532829 }, { 1, 0.0245469769662212 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.704702607811644 }, { 1, 0.0690815460416068 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00083067388376837 }, { 1, 0.990018614397687 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.167422259169371 }, { 1, 0.357780264564459 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0189854860794952 }, { 1, 0.848291653001702 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00838929356238663 }, { 1, 0.918181907942244 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0018920134271698 }, { 1, 0.976630404464975 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.990136730682396 }, { 1, 0.0026671038739167 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.938437506110058 }, { 1, 0.0154591339099562 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.993255936266246 }, { 1, 0.0018500215482441 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.658732454672986 }, { 1, 0.07930042385756 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.737220117135672 }, { 1, 0.0676577186646154 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0487546269060146 }, { 1, 0.656598380271274 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.882615964636337 }, { 1, 0.0267501906702354 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000403421522657179 }, { 1, 0.993942283503276 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.155116073518434 }, { 1, 0.432737506651976 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.995815258541467 }, { 1, 0.00124662855859921 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0258754743699977 }, { 1, 0.798625157645213 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000701507303933087 }, { 1, 0.989730402066815 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0789664837731301 }, { 1, 0.584806163262754 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.06882044962112 }, { 1, 0.612875308592813 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000380276277832859 }, { 1, 0.994964707117622 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0632553662563583 }, { 1, 0.612389276810728 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.194063889277119 }, { 1, 0.350467090400275 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.844854177593533 }, { 1, 0.0377856313045795 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.20031632135846 }, { 1, 0.393699528114068 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00248032992297501 }, { 1, 0.973194708208494 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0278005002001655 }, { 1, 0.821520041243683 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0734439511455673 }, { 1, 0.612436818776392 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.459497156322209 }, { 1, 0.160031553035316 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000466181138822702 }, { 1, 0.99314708993967 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00509105733882597 }, { 1, 0.944197033499934 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.340478301551183 }, { 1, 0.20874400731972 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000994017707436871 }, { 1, 0.986467760244021 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000274656235009955 }, { 1, 0.995595210263531 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.587232575620936 }, { 1, 0.105668219269439 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000443252891314864 }, { 1, 0.994026683139058 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000181391006671009 }, { 1, 0.997156912754577 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.965549093826312 }, { 1, 0.00915309804857792 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00242968647505077 }, { 1, 0.970979017547228 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0567903397133379 }, { 1, 0.700600755881461 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0887177518312932 }, { 1, 0.596741349081383 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.140867030502927 }, { 1, 0.415283317527037 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000665271179681572 }, { 1, 0.991838590539139 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.347153973096687 }, { 1, 0.219860310616286 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0012380190603183 }, { 1, 0.983612376900274 }, }), };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ClassificationStochasticGradientDecentModel_PredictProbability_Multiple()
        {
            var parser = new CsvParser(() => new StringReader(Resources.LogitTest));
            var observations = parser.EnumerateRows("F1", "F2").ToF64Matrix();
            var featureNameToIndex = parser.EnumerateRows("F1", "F2").First().ColumnNameToIndex;
            var targets = parser.EnumerateRows("Target").ToF64Vector();
            var rows = targets.Length;

            var learner = new ClassificationStochasticGradientDecentLearner(0.0001, 100000, 0.0, 42, 1);
            var sut = learner.Learn(observations, targets);

            var actual = sut.PredictProbability(observations);

            var evaluator = new TotalErrorClassificationMetric<double>();
            var error = evaluator.Error(targets, actual.Select(p => p.Prediction).ToArray());

            Assert.AreEqual(0.11, error, 0.0000001);
            var expected = new ProbabilityPrediction[] { new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.621101688132576 }, { 1, 0.0846881846950576 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.996775267107844 }, { 1, 0.000978576747651287 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.734571906910202 }, { 1, 0.0560691486719206 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0146128977972945 }, { 1, 0.864567303084922 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00465883607592212 }, { 1, 0.951584191188203 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.878121560867104 }, { 1, 0.0269925976131346 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00325193200227069 }, { 1, 0.959527740814359 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.289961572914445 }, { 1, 0.288441954290256 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00136926544946117 }, { 1, 0.982308673748164 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.141385058527512 }, { 1, 0.486304759053574 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0638775362194203 }, { 1, 0.686490500097714 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.780859048908629 }, { 1, 0.0589063271468531 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00252880042425468 }, { 1, 0.971519850769858 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00087001279403044 }, { 1, 0.98740619000205 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.518839497800345 }, { 1, 0.120766688297536 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0233345267349947 }, { 1, 0.802122627607638 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.288881083312414 }, { 1, 0.280995279161987 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.519797291829791 }, { 1, 0.142775277629942 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00138927920018171 }, { 1, 0.981417425379206 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.212813784372327 }, { 1, 0.369635773422515 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.663944289044126 }, { 1, 0.0898669644240356 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00383278151966456 }, { 1, 0.961609129679238 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.902332538939807 }, { 1, 0.0227177491768366 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.994182189991355 }, { 1, 0.0016750314168006 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0128151441950428 }, { 1, 0.890023451669696 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0924308234966023 }, { 1, 0.551463210142294 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.200333386383766 }, { 1, 0.390327696898008 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0848695208602149 }, { 1, 0.622509079619917 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.610815632228507 }, { 1, 0.103863511345075 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.842803823944141 }, { 1, 0.0331355416443153 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0726545903870118 }, { 1, 0.605067021309892 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0224314022017193 }, { 1, 0.839079865094142 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.516529956095135 }, { 1, 0.131192031376301 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.314151760775334 }, { 1, 0.238567128919405 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.654369918912128 }, { 1, 0.0782914222546364 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.767308909284225 }, { 1, 0.0549450288457183 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0966147800476709 }, { 1, 0.492180181887752 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0173290689348948 }, { 1, 0.849659037294936 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.453472079161749 }, { 1, 0.180062953612024 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.717906972566941 }, { 1, 0.059376160140945 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0303782758955656 }, { 1, 0.797779204189589 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.913160268853319 }, { 1, 0.0205073104224625 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00203157004984982 }, { 1, 0.97817527074423 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.246194313426826 }, { 1, 0.340831406377204 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.928453252401366 }, { 1, 0.0171059670547643 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.537630876318061 }, { 1, 0.13082767370801 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0116187961764023 }, { 1, 0.897660135429518 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 7.22906642570243E-05 }, { 1, 0.998775474649305 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00274401349818616 }, { 1, 0.965087830900596 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000132042182498753 }, { 1, 0.997853116657023 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00481206796842223 }, { 1, 0.950572498548318 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0019202922181901 }, { 1, 0.979858947815663 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0665401709392385 }, { 1, 0.670270309873586 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.947870386295566 }, { 1, 0.0114755278656634 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.893732847532829 }, { 1, 0.0245469769662212 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.704702607811644 }, { 1, 0.0690815460416068 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00083067388376837 }, { 1, 0.990018614397687 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.167422259169371 }, { 1, 0.357780264564459 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0189854860794952 }, { 1, 0.848291653001702 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00838929356238663 }, { 1, 0.918181907942244 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0018920134271698 }, { 1, 0.976630404464975 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.990136730682396 }, { 1, 0.0026671038739167 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.938437506110058 }, { 1, 0.0154591339099562 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.993255936266246 }, { 1, 0.0018500215482441 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.658732454672986 }, { 1, 0.07930042385756 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.737220117135672 }, { 1, 0.0676577186646154 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0487546269060146 }, { 1, 0.656598380271274 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.882615964636337 }, { 1, 0.0267501906702354 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000403421522657179 }, { 1, 0.993942283503276 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.155116073518434 }, { 1, 0.432737506651976 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.995815258541467 }, { 1, 0.00124662855859921 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0258754743699977 }, { 1, 0.798625157645213 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000701507303933087 }, { 1, 0.989730402066815 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0789664837731301 }, { 1, 0.584806163262754 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.06882044962112 }, { 1, 0.612875308592813 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000380276277832859 }, { 1, 0.994964707117622 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0632553662563583 }, { 1, 0.612389276810728 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.194063889277119 }, { 1, 0.350467090400275 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.844854177593533 }, { 1, 0.0377856313045795 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.20031632135846 }, { 1, 0.393699528114068 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00248032992297501 }, { 1, 0.973194708208494 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0278005002001655 }, { 1, 0.821520041243683 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0734439511455673 }, { 1, 0.612436818776392 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.459497156322209 }, { 1, 0.160031553035316 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000466181138822702 }, { 1, 0.99314708993967 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00509105733882597 }, { 1, 0.944197033499934 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.340478301551183 }, { 1, 0.20874400731972 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000994017707436871 }, { 1, 0.986467760244021 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000274656235009955 }, { 1, 0.995595210263531 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.587232575620936 }, { 1, 0.105668219269439 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000443252891314864 }, { 1, 0.994026683139058 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000181391006671009 }, { 1, 0.997156912754577 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.965549093826312 }, { 1, 0.00915309804857792 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.00242968647505077 }, { 1, 0.970979017547228 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0567903397133379 }, { 1, 0.700600755881461 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0887177518312932 }, { 1, 0.596741349081383 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.140867030502927 }, { 1, 0.415283317527037 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.000665271179681572 }, { 1, 0.991838590539139 }, }), new ProbabilityPrediction(0, new Dictionary<double, double> { { 0, 0.347153973096687 }, { 1, 0.219860310616286 }, }), new ProbabilityPrediction(1, new Dictionary<double, double> { { 0, 0.0012380190603183 }, { 1, 0.983612376900274 }, }), };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ClassificationStochasticGradientDecentModel_Save()
        {
            var parser = new CsvParser(() => new StringReader(Resources.LogitTest));
            var observations = parser.EnumerateRows("F1", "F2").ToF64Matrix();
            var targets = parser.EnumerateRows("Target").ToF64Vector();

            var learner = new ClassificationStochasticGradientDecentLearner(0.0001, 100000, 0.0, 42, 1);
            var sut = learner.Learn(observations, targets);

            var writer = new StringWriter();
            sut.Save(() => writer);
            
            Assert.AreEqual(ClassificationStochasticGradientDecentModelString, writer.ToString());
        }

        [TestMethod]
        public void ClassificationStochasticGradientDecentModel_Load()
        {
            var parser = new CsvParser(() => new StringReader(Resources.LogitTest));
            var observations = parser.EnumerateRows("F1", "F2").ToF64Matrix();
            var targets = parser.EnumerateRows("Target").ToF64Vector();

            var reader = new StringReader(ClassificationStochasticGradientDecentModelString);
            var sut = ClassificationStochasticGradientDecentModel.Load(() => reader);

            var predictions = sut.Predict(observations);

            var metric = new TotalErrorClassificationMetric<double>();
            var actual = metric.Error(targets, predictions);
            Assert.AreEqual(0.11, actual, 0.001);
        }

        readonly string ClassificationStochasticGradientDecentModelString =
            "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ClassificationStochasticGradientDecentModel xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" z:Id=\"1\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Linear.Models\">\r\n  <m_models xmlns:d2p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" z:Id=\"2\" z:Size=\"2\">\r\n    <d2p1:KeyValueOfdoubleBinaryClassificationStochasticGradientDecentModelexXrXUEF>\r\n      <d2p1:Key>0</d2p1:Key>\r\n      <d2p1:Value z:Id=\"3\">\r\n        <m_weights z:Id=\"4\" z:Size=\"3\">\r\n          <d2p1:double>15.93428828357912</d2p1:double>\r\n          <d2p1:double>-0.14011316001536853</d2p1:double>\r\n          <d2p1:double>-0.13571128043372763</d2p1:double>\r\n        </m_weights>\r\n      </d2p1:Value>\r\n    </d2p1:KeyValueOfdoubleBinaryClassificationStochasticGradientDecentModelexXrXUEF>\r\n    <d2p1:KeyValueOfdoubleBinaryClassificationStochasticGradientDecentModelexXrXUEF>\r\n      <d2p1:Key>1</d2p1:Key>\r\n      <d2p1:Value z:Id=\"5\">\r\n        <m_weights z:Id=\"6\" z:Size=\"3\">\r\n          <d2p1:double>-15.933910846987333</d2p1:double>\r\n          <d2p1:double>0.12772766551767137</d2p1:double>\r\n          <d2p1:double>0.11702991205748456</d2p1:double>\r\n        </m_weights>\r\n      </d2p1:Value>\r\n    </d2p1:KeyValueOfdoubleBinaryClassificationStochasticGradientDecentModelexXrXUEF>\r\n  </m_models>\r\n</ClassificationStochasticGradientDecentModel>";

    }
}
