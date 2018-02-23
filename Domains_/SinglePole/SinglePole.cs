using System;

namespace MyNEAT.Domains.SinglePole
{
    /// <summary>
    ///     Model state variables for single pole balancing task.
    /// </summary>
    public class SinglePoleStateData
    {
        /// <summary>
        ///     Action applied during most recent timestep.
        /// </summary>
        public bool _action;

        /// <summary>
        ///     Cart position (meters from origin).
        /// </summary>
        public double _cartPosX;

        /// <summary>
        ///     Cart velocity (m/s).
        /// </summary>
        public double _cartVelocityX;

        public bool _done;

        /// <summary>
        ///     Pole angle (radians). Straight up = 0.
        /// </summary>
        public double _poleAngle;

        /// <summary>
        ///     Pole angular velocity (radians/sec).
        /// </summary>
        public double _poleAngularVelocity;

        public float _reward;
    }

    public class SinglePoleBalancingEnvironment
    {
        /// <summary>
        ///     Calculates a state update for the next timestep using current model state and a single 'action' from the
        ///     controller. The action specifies if the controller is pushing the cart left or right. Note that this is a binary
        ///     action and therefore full force is always applied to the cart in some direction. This is the standard model for
        ///     the single pole balancing task.
        /// </summary>
        /// <param name="action">push direction, left(false) or right(true). Force magnitude is fixed.</param>
        public SinglePoleStateData SimulateTimestep(bool action)
        {
            stepsPassed += 1;
            //float xacc,thetaacc,force,costheta,sintheta,temp;
            var force = action ? ForceMag : -ForceMag;
            var cosTheta = Math.Cos(currState._poleAngle);
            var sinTheta = Math.Sin(currState._poleAngle);
            var tmp = (force + PoleMassLength * currState._poleAngularVelocity * currState._poleAngularVelocity *
                       sinTheta) / TotalMass;

            var thetaAcc = (Gravity * sinTheta - cosTheta * tmp)
                           / (Length * (FourThirds - MassPole * cosTheta * cosTheta / TotalMass));

            var xAcc = tmp - PoleMassLength * thetaAcc * cosTheta / TotalMass;


            // Update the four state variables, using Euler's method.
            currState._cartPosX += TimeDelta * currState._cartVelocityX;
            currState._cartVelocityX += TimeDelta * xAcc;
            currState._poleAngle += TimeDelta * currState._poleAngularVelocity;
            currState._poleAngularVelocity += TimeDelta * thetaAcc;
            currState._action = action;
            currState._reward = stepsPassed;
            currState._done = currState._cartPosX < -_trackLengthHalf ||
                              currState._cartPosX > _trackLengthHalf ||
                              currState._poleAngle > _poleAngleThreshold ||
                              currState._poleAngle < -_poleAngleThreshold ||
                              stepsPassed > _maxTimesteps;

            return currState;
        }

        #region Constants

        // Some physical model constants.
        public const double Gravity = 9.8;

        public const double MassCart = 1.0;
        public const double MassPole = 0.1;
        public const double TotalMass = MassPole + MassCart;
        public const double Length = 0.5; // actually half the pole's length.
        public const double PoleMassLength = MassPole * Length;
        public const double ForceMag = 10.0;

        /// <summary>Time increment interval in seconds.</summary>
        public const double TimeDelta = 0.02;

        public const double FourThirds = 4.0 / 3.0;

        // Some precalced angle constants.
        public const double OneDegree = Math.PI / 180.0; //= 0.0174532;

        public const double SixDegrees = Math.PI / 30.0; //= 0.1047192;
        public const double TwelveDegrees = Math.PI / 15.0; //= 0.2094384;
        public const double TwentyFourDegrees = Math.PI / 7.5; //= 0.2094384;
        public const double ThirtySixDegrees = Math.PI / 5.0; //= 0.628329;
        public const double FiftyDegrees = Math.PI / 3.6; //= 0.87266;

        #endregion

        #region Class Variables

        // Domain parameters.
        public SinglePoleStateData currState;

        private int stepsPassed;

        public double _trackLength;
        public double _trackLengthHalf;
        public int _maxTimesteps;
        public double _poleAngleThreshold;

        #endregion

        #region Constructors

        /// <summary>
        ///     Construct evaluator with default task arguments/variables.
        /// </summary>
        public SinglePoleBalancingEnvironment() : this(4.8, 1000, TwelveDegrees)
        {
        }

        /// <summary>
        ///     Construct evaluator with the provided task arguments/variables.
        /// </summary>
        public SinglePoleBalancingEnvironment(double trackLength, int maxTimesteps, double poleAngleThreshold)
        {
            _trackLength = trackLength;
            _trackLengthHalf = trackLength / 2.0;
            _maxTimesteps = maxTimesteps;
            _poleAngleThreshold = poleAngleThreshold;
            currState = new SinglePoleStateData();
            currState._poleAngle = SixDegrees;
            stepsPassed = 0;
        }

        #endregion
    }
}