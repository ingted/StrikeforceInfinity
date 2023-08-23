﻿using HG.Actors.Objects;
using HG.Engine;
using HG.Types;
using Determinet;
using Determinet.Types;
using Hells_Genesis.ExtensionMethods;
using System;
using System.IO;
using static HG.Engine.Constants;

namespace HG.AI.Logistics
{
    /// <summary>
    /// AI brain to keep an object close to another object, but at a generally safe distance.
    /// </summary>
    internal class Meander : IAIController
    {
        private readonly Core _core;
        private readonly ActorBase _owner;
        private readonly ActorBase _observedObject;

        #region I/O Enumerations.

        public enum Inputs
        {
            DistanceFromObservationObject,
            /// <summary>
            /// This should be the angle to the ObservationObject (likely the player) in relation to the nose of the enemy ship expressed in 1/6th radians split in half.
            /// 0 is pointing at the player, ~0.26 is 90° to the right, ~0.523 is 180° and -0.26 is 270° to the left.
            /// </summary>
            AngleToObservationObjectIn6thRadians
        }

        public enum Outputs
        {
            TransitionToObservationObject,
            TransitionFromObservationObject,
            SpeedAdjust
        }

        #endregion

        #region Instance parameters.

        public double DistanceToKeep { get; set; } = 500;
        public DateTime? LastDecisionTime { get; set; } = DateTime.Now.AddHours(-1);
        public int MillisecondsBetweenDecisions { get; set; } = 50;
        public RelativeDirection FavorateDirection = RelativeDirection.None;

        #endregion

        public DniNeuralNetwork Network { get; set; }

        private static DniNeuralNetwork _singletonNetwork = null;

        /// <summary>
        /// Creates a new instance of the intelligence object.
        /// </summary>
        /// <param name="core">Engine core instance.</param>
        /// <param name="owner">The object which is intelligent.</param>
        /// <param name="observedObject">The object for which the intelligent object will be observing for inputs.</param>
        /// <param name="pretrainedModelFile">If there is a pre-trained model, this would be the file.</param>
        public Meander(Core core, ActorBase owner, ActorBase observedObject, string pretrainedModelFile = null)
        {
            _core = core;
            _owner = owner;
            _observedObject = observedObject;
            FavorateDirection = HGRandom.FlipCoin() ? RelativeDirection.Left : RelativeDirection.Right;

            if (_singletonNetwork != null)
            {
                Network = _singletonNetwork.Clone();//.Mutate(0.2, 0.1);
                return;
            }

            if (string.IsNullOrEmpty(pretrainedModelFile) == false && File.Exists(pretrainedModelFile))
            {
                var loadedNetwork = DniNeuralNetwork.Load(pretrainedModelFile);
                if (loadedNetwork != null)
                {
                    _singletonNetwork = loadedNetwork;
                    Network = loadedNetwork.Clone();//.Mutate(0.2, 0.1)
                    return;
                }
            }

            #region New neural network and training.

            var newNetwork = new DniNeuralNetwork()
            {
                LearningRate = 0.01
            };

            //Vision inputs.
            newNetwork.Layers.AddInput(ActivationType.LeakyReLU,
                new object[] {
                        Inputs.DistanceFromObservationObject,
                        Inputs.AngleToObservationObjectIn6thRadians
                });

            //Where the magic happens.
            newNetwork.Layers.AddIntermediate(ActivationType.Sigmoid, 8);

            //Decision outputs
            newNetwork.Layers.AddOutput(
                new object[] {
                        Outputs.TransitionToObservationObject,
                        Outputs.TransitionFromObservationObject,
                        Outputs.SpeedAdjust
                });

            for (int epoch = 0; epoch < 5000; epoch++)
            {
                //Very close to observed object, get away.
                newNetwork.BackPropagate(TrainingScenerio(0, 0), TrainingDecision(0, 1, 1));
                newNetwork.BackPropagate(TrainingScenerio(0, -1), TrainingDecision(0, 1, 1));
                newNetwork.BackPropagate(TrainingScenerio(0, 1), TrainingDecision(0, 1, 1));
                newNetwork.BackPropagate(TrainingScenerio(0, 0.5), TrainingDecision(0, 1, 1));
                newNetwork.BackPropagate(TrainingScenerio(0, -0.5), TrainingDecision(0, 1, 1));

                //Pretty close to observed object, get away.
                newNetwork.BackPropagate(TrainingScenerio(0.25, 0), TrainingDecision(0, 1, 0.5));
                newNetwork.BackPropagate(TrainingScenerio(0.25, -1), TrainingDecision(0, 1, 0.5));
                newNetwork.BackPropagate(TrainingScenerio(0.25, 1), TrainingDecision(0, 1, 0.5));
                newNetwork.BackPropagate(TrainingScenerio(0.25, 0.5), TrainingDecision(0, 1, 0.5));
                newNetwork.BackPropagate(TrainingScenerio(0.25, -0.5), TrainingDecision(0, 1, 0.5));

                //Very far from observed object, get closer.
                newNetwork.BackPropagate(TrainingScenerio(1, 0), TrainingDecision(1, 0, 1));
                newNetwork.BackPropagate(TrainingScenerio(1, -1), TrainingDecision(1, 0, 1));
                newNetwork.BackPropagate(TrainingScenerio(1, 1), TrainingDecision(1, 0, 1));
                newNetwork.BackPropagate(TrainingScenerio(1, 0.5), TrainingDecision(1, 0, 1));
                newNetwork.BackPropagate(TrainingScenerio(1, -0.5), TrainingDecision(1, 0, 1));

                //Pretty far from observed object, get closer.
                newNetwork.BackPropagate(TrainingScenerio(0.75, 0), TrainingDecision(1, 0, 0.5));
                newNetwork.BackPropagate(TrainingScenerio(0.75, -1), TrainingDecision(1, 0, 0.5));
                newNetwork.BackPropagate(TrainingScenerio(0.75, 1), TrainingDecision(1, 0, 0.5));
                newNetwork.BackPropagate(TrainingScenerio(0.75, 0.5), TrainingDecision(1, 0, 0.5));
                newNetwork.BackPropagate(TrainingScenerio(0.75, -0.5), TrainingDecision(1, 0, 0.5));
            }

            static DniNamedInterfaceParameters TrainingScenerio(double distanceFromObservationObject, double angleToObservationObjectIn10thRadians)
            {
                var param = new DniNamedInterfaceParameters();
                param.Set(Inputs.DistanceFromObservationObject, distanceFromObservationObject);
                param.Set(Inputs.AngleToObservationObjectIn6thRadians, angleToObservationObjectIn10thRadians);
                return param;
            }

            static DniNamedInterfaceParameters TrainingDecision(double transitionToObservationObject, double transitionFromObservationObject, double speedAdjust)
            {
                var param = new DniNamedInterfaceParameters();
                param.Set(Outputs.TransitionToObservationObject, transitionToObservationObject);
                param.Set(Outputs.TransitionFromObservationObject, transitionFromObservationObject);
                param.Set(Outputs.SpeedAdjust, speedAdjust);
                return param;
            }

            //newNetwork.Save(fileName);

            #endregion

            _singletonNetwork = newNetwork;
            Network = newNetwork.Clone();//.Mutate(0.2, 0.1)
        }

        public void ApplyIntelligence(HGPoint<double> frameAppliedOffset)
        {
            var now = DateTime.UtcNow;

            var elapsedTimeSinceLastDecision = (now - (DateTime)LastDecisionTime).TotalMilliseconds;

            if (elapsedTimeSinceLastDecision >= MillisecondsBetweenDecisions)
            {
                if (elapsedTimeSinceLastDecision > 1000)
                {
                    FavorateDirection = HGRandom.FlipCoin() ? RelativeDirection.Left : RelativeDirection.Right;
                }

                var decidingFactors = GatherInputs();
                var decisions = Network.FeedForward(decidingFactors);

                var speedAdjust = decisions.Get(Outputs.SpeedAdjust);

                _owner.Velocity.ThrottlePercentage += (speedAdjust / 5.0);

                bool transitionToObservationObject = decisions.Get(Outputs.TransitionToObservationObject) > 0.9;
                bool transitionFromObservationObject = decisions.Get(Outputs.TransitionFromObservationObject) > 0.9;

                if (transitionToObservationObject && transitionFromObservationObject)
                {
                }
                else if (transitionToObservationObject)
                {
                    _owner.Rotate((45 * 0.05) * (FavorateDirection == RelativeDirection.Left ? 1 : -1));
                }
                else if (transitionFromObservationObject)
                {
                    _owner.Rotate((-45 * 0.05) * (FavorateDirection == RelativeDirection.Left ? 1 : -1));
                }

                LastDecisionTime = now;
            }
        }

        private DniNamedInterfaceParameters GatherInputs()
        {
            var aiParams = new DniNamedInterfaceParameters();

            var distance = _owner.DistanceTo(_observedObject);
            var percentageOfCloseness = ((100 - ((distance / DistanceToKeep) * 100.0)) / 100.0).Box(0, 1);

            aiParams.Set(Inputs.DistanceFromObservationObject, percentageOfCloseness);

            var deltaAngle = _owner.DeltaAngle(_observedObject);

            var angleToIn6thRadians = HGAngle<double>.DegreesToRadians(deltaAngle) / 6.0;

            aiParams.Set(Inputs.AngleToObservationObjectIn6thRadians,
                angleToIn6thRadians.SplitToNegative(Math.PI / 6));

            return aiParams;
        }
    }
}