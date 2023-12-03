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

    float mapWidth = 129.19f; // 真实场景尺寸  通过配置得到

    float mapHeight = 129.19f;

    float playerPosX;//玩家位置

    float playerPosY ;

    float map_xScale;//地图缩放

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

    //初始化
    public void Init(NpcMapConfig config , Transform playerTrans ,Transform cameraTrans)
    {
        //拿到地图数据  设置地图尺寸和名称
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
    //计算地图缩放
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

    //更新小地图显示
    private void UpdateMap()
    {
        //更新玩家位置显示

        float posPlayerX = (playerTrans.localPosition.x+ mapWidth / 2) / mapWidth;

        float posPlayerY = (playerTrans.localPosition.z + mapHeight / 2) / mapHeight;

        mapImage.uvRect = new Rect(posPlayerX - map_xScale / 2, posPlayerY - map_yScale / 2, map_xScale, map_yScale);

        //更新玩家方向显示
        playerArrow.rotation = Quaternion.Euler(0, 0, -playerTrans.localEulerAngles.y);
        //更新相机视场显示
        cameraFov.rotation = Quaternion.Euler(0, 0, -cameraTrans.localEulerAngles.y);

        //更新icon显示
    }
}
