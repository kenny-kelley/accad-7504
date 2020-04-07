using UnityEngine;

namespace Passer {

    [System.Serializable]
    public class MicrophoneHead : UnityHeadSensor {

        // This device also implements audio source clips processing
        private AudioSource audioSource;

        private string device;
        private const int sampleRate = 44100;
        private AudioClip clipRecord;
        private readonly int sampleWindow = 128;

        public override void Start(HumanoidControl _humanoid, Transform targetTransform) {
            base.Start(_humanoid, targetTransform);

            // audio sources are always process, even when the microphone is disabled
            audioSource = headTarget.GetComponent<AudioSource>();

            if (!enabled || !headTarget.unityVRHead.enabled)
                return;

#if hKINECT
            // Microphone device is not compatible with kinect microphone
            // When kinect is enabled, it will use audio input from kinect
            if (headTarget.humanoid.kinectTracker.enabled)
                return;
#endif
            if (device == null && Microphone.devices != null && Microphone.devices.Length > 0)
                device = Microphone.devices[0];

            if (device != null)
                clipRecord = Microphone.Start(device, true, 999, 44100);
        }

        public void StopSensor() {
            // Microphone device is not compatible with kinect microphone
            // When kinect is enabled, it will use audio input from kinect
            //#if !hKINECT
            Microphone.End(device);
            //#endif
        }

        public override void Update() {
            float volume = LevelMaxAudioSource();

            // Microphone device is not compatible with kinect microphone
            // When kinect is enabled, it will use audio input from kinect
            //#if !hKINECT
            if (enabled && headTarget.unityVRHead.enabled)
                volume += LevelMax();
            headTarget.audioEnergy = volume * 256;
            //#endif
        }

        private float LevelMaxAudioSource() {
            if (audioSource == null)
                return 0;

            float levelMax = 0;
            float[] waveData = new float[sampleWindow];
            if (audioSource.isPlaying && audioSource.timeSamples < audioSource.clip.samples)
                audioSource.clip.GetData(waveData, audioSource.timeSamples);
            // Getting a peak on the last 128 samples
            for (int i = 0; i < sampleWindow; i++) {
                float wavePeak = waveData[i] * waveData[i];
                if (levelMax < wavePeak)
                    levelMax = wavePeak;
            }
            return levelMax;
        }

        private float LevelMax() {
            float levelMax = 0;
            float[] waveData = new float[sampleWindow];
            int microphonePosition = Microphone.GetPosition(null) - (sampleWindow + 1); // null means the first microphone
            if (microphonePosition < 0 || clipRecord == null)
                return 0;

            clipRecord.GetData(waveData, microphonePosition);
            // Getting a peak on the last 128 samples
            for (int i = 0; i < sampleWindow; i++) {
                float wavePeak = waveData[i] * waveData[i];
                if (levelMax < wavePeak)
                    levelMax = wavePeak;
            }
            return levelMax;
        }

        /*
        bool _isInitialized;
        // start mic when scene starts
        void OnEnable()
        {
            InitMic();
            _isInitialized=true;
        }
     
        //stop mic when loading a new level or quit application
        void OnDisable()
        {
            StopMicrophone();
        }
     
        void OnDestroy()
        {
            StopMicrophone();
        }
     
     
        // make sure the mic gets started & stopped when application gets focused
        void OnApplicationFocus(bool focus) {
            if (focus)
            {
                //Debug.Log("Focus");
             
                if(!_isInitialized){
                    //Debug.Log("Init Mic");
                    InitMic();
                    _isInitialized=true;
                }
            }      
            if (!focus)
            {
                //Debug.Log("Pause");
                StopMicrophone();
                //Debug.Log("Stop Mic");
                _isInitialized=false;
             
            }
        }
        */
    }
}
