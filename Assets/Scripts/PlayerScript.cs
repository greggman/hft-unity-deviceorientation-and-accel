using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;
using CSSParse;

class PlayerScript : MonoBehaviour {
    // Classes based on MessageCmdData are automatically registered for deserialization
    // by CmdName.
    [CmdName("color")]
    private class MessageColor : MessageCmdData {
        public string color = "";    // in CSS format rgb(r,g,b)
    };

    [CmdName("move")]
    private class MessageMove : MessageCmdData {
        public float x = 0;
        public float y = 0;
    };

    [CmdName("accel")]
    private class MessageAccel : MessageCmdData {
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float a = 0;
        public float b = 0;
        public float g = 0;
    };

    [CmdName("rot")]
    private class MessageRot : MessageCmdData {
        public float x = 0;
        public float y = 0;
        public float z = 0;
    };

    [CmdName("setName")]
    private class MessageSetName : MessageCmdData {
        public MessageSetName() {  // needed for deserialization
        }
        public MessageSetName(string _name) {
            name = _name;
        }
        public string name = "";
    };

    [CmdName("busy")]
    private class MessageBusy : MessageCmdData {
        public bool busy = false;
    }

    // NOTE: This message is only sent, never received
    // therefore it does not need a no parameter constructor.
    // If you do receive one you'll get an error unless you
    // add a no parameter constructor.
    [CmdName("scored")]
    private class MessageScored : MessageCmdData {
        public MessageScored(int _points) {
            points = _points;
        }

        public int points;
    }

    void InitializeNetPlayer(SpawnInfo spawnInfo) {
        m_netPlayer = spawnInfo.netPlayer;
        m_netPlayer.OnDisconnect += Remove;

        // Setup events for the different messages.
        m_netPlayer.RegisterCmdHandler<MessageColor>(OnColor);
        m_netPlayer.RegisterCmdHandler<MessageMove>(OnMove);
        m_netPlayer.RegisterCmdHandler<MessageSetName>(OnSetName);
        m_netPlayer.RegisterCmdHandler<MessageBusy>(OnBusy);
        m_netPlayer.RegisterCmdHandler<MessageAccel>(OnAccel);
        m_netPlayer.RegisterCmdHandler<MessageRot>(OnRot);

        GameSettings settings = GameSettings.settings();
        m_position = new Vector3(Random.value * settings.areaWidth, 0, Random.value * settings.areaHeight);
        transform.localPosition = m_position;

        SetName(spawnInfo.name);
    }

    void Start() {
        m_position = gameObject.transform.localPosition;
        m_color = new Color(0.0f, 1.0f, 0.0f);
        // Look up renderer from mesh
        renderer.material.color = m_color;
    }

    public void Update() {
    }

    private void SetName(string name) {
        m_name = name;
    }

    public void OnTriggerEnter(Collider other) {
        // Because of physics layers we can only collide with the goal
        m_netPlayer.SendCmd(new MessageScored((int)(Random.Range(5f, 15f))));
    }

    private void Remove(object sender, System.EventArgs e) {
        Destroy(gameObject);
    }

    private void OnColor(MessageColor data) {
        m_color = CSSParse.Style.ParseCSSColor(data.color);
    }

    private void OnMove(MessageMove data) {
        float dx = data.x;
        float dy = data.y;
        GameSettings settings = GameSettings.settings();
        m_position.x = dx * settings.areaWidth;
        m_position.z = settings.areaHeight - (dy * settings.areaHeight) - 1;  // because in 2D down is positive.

        gameObject.transform.localPosition = m_position;
    }

    private void OnSetName(MessageSetName data) {
        if (data.name.Length == 0) {
            m_netPlayer.SendCmd(new MessageSetName(m_name));
        } else {
            SetName(data.name);
        }
    }

    private void OnBusy(MessageBusy data) {
        // not used.
    }

    private void OnAccel(MessageAccel data) {
        if (data.y > 0) {
            transform.localPosition = transform.localPosition + transform.forward * data.y * 0.01f;
        }
    }

    private void OnRot(MessageRot data) {
        transform.localEulerAngles = new Vector3(data.x, data.y, data.z);
    }

    public int order = 0;
    public Vector3 rotMult = new Vector3(1,1,1);

    private NetPlayer m_netPlayer;
    private Vector3 m_position;
    private Color m_color;
    private string m_name;
}


