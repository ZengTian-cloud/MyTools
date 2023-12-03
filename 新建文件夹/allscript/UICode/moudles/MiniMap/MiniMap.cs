using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Org.BouncyCastle.Math.EC.ECCurve;

public class MiniMap : MonoBehaviour
{
    RawImage mapImage;

    Transform playerArrow;

    Transform cameraFov;

    TextMeshProUGUI mapName;

    float mapWidth = 129.19f; // ��ʵ�����ߴ�  ͨ�����õõ�

    float mapHeight = 129.19f;

    float playerPosX;//���λ��

    float playerPosY ;

    float map_xScale;//��ͼ����

    float map_yScale;

    NpcMapConfig config;

    Transform playerTrans, cameraTrans;

    private void Awake()
    {
        mapImage = GetComponent<RawImage>();

        playerArrow = transform.Find("playerArrow");

        cameraFov = transform.Find("cameraFov");

        mapName = Utils.Find<TextMeshProUGUI>(transform, "mapName");
    }

    //��ʼ��
    public void Init(NpcMapConfig config , Transform playerTrans ,Transform cameraTrans)
    {
        //�õ���ͼ����  ���õ�ͼ�ߴ������
        this.config = config;

        this.playerTrans = playerTrans;

        this.cameraTrans = cameraTrans;

        mapWidth = config.Size.x;

        mapHeight = config.Size.y;

        mapName.SetText(GameCenter.mIns.m_LanMgr.GetLan(config.name));

        LoadMapImage();

        CalculateMapScale();
    }

    async void LoadMapImage()
    {
        mapImage.texture = await SpriteManager.Instance.GetTextureSync(config.texturePath);
    }
    //�����ͼ����
    void CalculateMapScale()
    {
        float mapImageWidth = mapImage.texture.width;

        float mapImageHeight = mapImage.texture.height;

        RectTransform rectTrans = transform.GetComponent<RectTransform>();

        float rectWidth = rectTrans.rect.width;

        float rectHeight = rectTrans.rect.height;

        map_xScale = rectWidth / mapImageWidth;

        map_yScale = rectHeight / mapImageHeight;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            playerPosY++;
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerPosY--;
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerPosX--;
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerPosX++;
        }
        UpdateMap();
    }

    //����С��ͼ��ʾ
    private void UpdateMap()
    {
        //�������λ����ʾ

        float posPlayerX = (playerTrans.localPosition.x+ mapWidth / 2) / mapWidth;

        float posPlayerY = (playerTrans.localPosition.z + mapHeight / 2) / mapHeight;

        mapImage.uvRect = new Rect(posPlayerX - map_xScale / 2, posPlayerY - map_yScale / 2, map_xScale, map_yScale);

        //������ҷ�����ʾ
        playerArrow.rotation = Quaternion.Euler(0, 0, -playerTrans.localEulerAngles.y);
        //��������ӳ���ʾ
        cameraFov.rotation = Quaternion.Euler(0, 0, -cameraTrans.localEulerAngles.y);

        //����icon��ʾ
    }
}
