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

    void Init() {
        if (m_renderer == null) {
            m_renderer = GetComponent<Renderer>();
        }
    }

    void InitializeNetPlayer(SpawnInfo spawnInfo) {
        Init();

        m_netPlayer = spawnInfo.netPlayer;
        m_netPlayer.OnDisconnect += Remove;
        m_netPlayer.OnNameChange += ChangeName;

        // Setup events for the different messages.
        m_netPlayer.RegisterCmdHandler<MessageColor>(OnColor);
        m_netPlayer.RegisterCmdHandler<MessageMove>(OnMove);
        m_netPlayer.RegisterCmdHandler<MessageAccel>(OnAccel);
        m_netPlayer.RegisterCmdHandler<MessageRot>(OnRot);

        GameSettings settings = GameSettings.settings();
        m_position = new Vector3(Random.value * settings.areaWidth, 0, Random.value * settings.areaHeight);
        transform.localPosition = m_position;

        SetName(m_netPlayer.Name);
    }

    void Start() {
        Init();
        m_position = gameObject.transform.localPosition;
        SetColor(new Color(0.0f, 1.0f, 0.0f));
    }

    public void Update() {
    }

    private void SetColor(Color color) {
        m_color = color;
        m_renderer.material.color = m_color;
        Color[] pix = new Color[1];
        pix[0] = color;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixels(pix);
        tex.Apply();
        m_guiStyle.normal.background = tex;
    }

    void OnGUI()
    {
        Vector2 size = m_guiStyle.CalcSize(m_guiName);
        Vector3 coords = Camera.main.WorldToScreenPoint(transform.position);
        m_nameRect.x = coords.x - size.x * 0.5f - 5.0f;
        m_nameRect.y = Screen.height - coords.y - 40.0f;
        GUI.Box(m_nameRect, m_name, m_guiStyle);
    }

    void SetName(string name) {
        m_name = name;
        gameObject.name = "Player-" + m_name;
        m_guiName = new GUIContent(m_name);
        m_guiStyle.normal.textColor = Color.black;
        m_guiStyle.contentOffset = new Vector2(4.0f, 2.0f);
        Vector2 size = m_guiStyle.CalcSize(m_guiName);
        m_nameRect.width  = size.x + 12;
        m_nameRect.height = size.y + 5;
    }

    public void OnTriggerEnter(Collider other) {
        // Because of physics layers we can only collide with the goal
        m_netPlayer.SendCmd(new MessageScored((int)(Random.Range(5f, 15f))));
    }

    private void Remove(object sender, System.EventArgs e) {
        Destroy(gameObject);
    }

    private void OnColor(MessageColor data) {
        SetColor(CSSParse.Style.ParseCSSColor(data.color));
    }

    private void OnMove(MessageMove data) {
        float dx = data.x;
        float dy = data.y;
        GameSettings settings = GameSettings.settings();
        m_position.x = dx * settings.areaWidth;
        m_position.z = settings.areaHeight - (dy * settings.areaHeight) - 1;  // because in 2D down is positive.

        gameObject.transform.localPosition = m_position;
    }

    private void ChangeName(object sender, System.EventArgs e) {
        SetName(m_netPlayer.Name);
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

    private Renderer m_renderer;
    private NetPlayer m_netPlayer;
    private Vector3 m_position;
    private Color m_color;
    private string m_name;
    private GUIStyle m_guiStyle = new GUIStyle();
    private GUIContent m_guiName = new GUIContent("");
    private Rect m_nameRect = new Rect(0,0,0,0);
}


