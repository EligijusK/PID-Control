using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Hands;

    /// <summary>
    /// Behavior that provides events for when an <see cref="XRHand"/> starts and ends a poke gesture. The gesture is
    /// detected if the index finger is extended and the middle, ring, and little fingers are curled in.
    /// </summary>
    public class ThumbsUpGestureDetector : MonoBehaviour
    {
        [SerializeField] [Tooltip("Which hand to check for the poke gesture.")]
        Handedness m_Handedness;

        [SerializeField] [Tooltip("Called when the hand has started a poke gesture.")]
        UnityEvent m_PokeGestureStarted;

        [SerializeField] [Tooltip("Called when the hand has ended a poke gesture.")]
        UnityEvent m_PokeGestureEnded;


        XRHandSubsystem m_Subsystem;
        bool m_IsPoking;

        static readonly List<XRHandSubsystem> s_Subsystems = new List<XRHandSubsystem>();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {

            SubsystemManager.GetSubsystems(s_Subsystems);
            if (s_Subsystems.Count == 0)
                return;

            m_Subsystem = s_Subsystems[0];
            m_Subsystem.updatedHands += OnUpdatedHands;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            if (m_Subsystem == null)
                return;

            m_Subsystem.updatedHands -= OnUpdatedHands;
            m_Subsystem = null;
        }


        void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            var wasPoking = m_IsPoking;
            switch (m_Handedness)
            {
                case Handedness.Left:
                    if (!HasUpdateSuccessFlag(updateSuccessFlags, XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints))
                        return;

                    var leftHand = subsystem.leftHand;
                    m_IsPoking = IsThumbExtended(leftHand) && IsIndexGrabbing(leftHand) && IsMiddleGrabbing(leftHand) && IsRingGrabbing(leftHand) &&
                        IsLittleGrabbing(leftHand);
                    break;
                
                case Handedness.Right:
                    if (!HasUpdateSuccessFlag(updateSuccessFlags, XRHandSubsystem.UpdateSuccessFlags.RightHandJoints))
                        return;

                    var rightHand = subsystem.rightHand;
                    m_IsPoking = IsThumbExtended(rightHand) && IsIndexGrabbing(rightHand) && IsMiddleGrabbing(rightHand) && IsRingGrabbing(rightHand) &&
                                IsLittleGrabbing(rightHand);
                    break;
            }

            if (m_IsPoking && !wasPoking)
                StartPokeGesture();
            else if (!m_IsPoking && wasPoking)
                EndPokeGesture();
        }

        /// <summary>
        /// Determines whether one or more bit fields are set in the flags.
        /// Non-boxing version of <c>HasFlag</c> for <see cref="XRHandSubsystem.UpdateSuccessFlags"/>.
        /// </summary>
        /// <param name="successFlags">The flags enum instance.</param>
        /// <param name="successFlag">The flag to check if set.</param>
        /// <returns>Returns <see langword="true"/> if the bit field or bit fields are set, otherwise returns <see langword="false"/>.</returns>
        static bool HasUpdateSuccessFlag(XRHandSubsystem.UpdateSuccessFlags successFlags, XRHandSubsystem.UpdateSuccessFlags successFlag)
        {
            return (successFlags & successFlag) == successFlag;
        }

        static bool IsThumbExtended(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.ThumbProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        static bool IsIndexGrabbing(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.IndexProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        /// <summary>
        /// Returns true if the given hand's middle finger tip is closer to the wrist than the middle proximal joint.
        /// </summary>
        /// <param name="hand">Hand to check for the required pose.</param>
        /// <returns>True if the given hand's middle finger tip is closer to the wrist than the middle proximal joint, false otherwise.</returns>
        static bool IsMiddleGrabbing(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.MiddleProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        /// <summary>
        /// Returns true if the given hand's ring finger tip is closer to the wrist than the ring proximal joint.
        /// </summary>
        /// <param name="hand">Hand to check for the required pose.</param>
        /// <returns>True if the given hand's ring finger tip is closer to the wrist than the ring proximal joint, false otherwise.</returns>
        static bool IsRingGrabbing(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.RingTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.RingProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        /// <summary>
        /// Returns true if the given hand's little finger tip is closer to the wrist than the little proximal joint.
        /// </summary>
        /// <param name="hand">Hand to check for the required pose.</param>
        /// <returns>True if the given hand's little finger tip is closer to the wrist than the little proximal joint, false otherwise.</returns>
        static bool IsLittleGrabbing(XRHand hand)
        {
            if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
                  hand.GetJoint(XRHandJointID.LittleTip).TryGetPose(out var tipPose) &&
                  hand.GetJoint(XRHandJointID.LittleProximal).TryGetPose(out var proximalPose)))
            {
                return false;
            }

            var wristToTip = tipPose.position - wristPose.position;
            var wristToProximal = proximalPose.position - wristPose.position;
            return wristToProximal.sqrMagnitude >= wristToTip.sqrMagnitude;
        }

        void StartPokeGesture()
        {
            m_IsPoking = true;
            m_PokeGestureStarted.Invoke();
        }

        void EndPokeGesture()
        {
            m_IsPoking = false;
            m_PokeGestureEnded.Invoke();
        }

    }