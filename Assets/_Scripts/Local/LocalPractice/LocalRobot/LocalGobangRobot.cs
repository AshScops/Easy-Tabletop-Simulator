using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tabletop.Local
{
    public class LocalGobangRobot : IRobot
    {
        public LocalGobangRobot(GoChessColor color, LocalGoChessBasket chessBasket, LocalMapObj map)
        {
            InitValueMap();
            InitWeights(map.Grids.Width, map.Grids.Height);

            m_robotColor = color;
            m_chessBasket = chessBasket;
            m_map = map;

            m_map.CurrentColor.Register((color) =>
            {
                if (color == m_robotColor)
                {
                    map.StartCoroutine(OnTurnToRobot());
                }
            }).UnRegisterWhenGameObjectDestroyed(map);
        }

        private GoChessColor m_robotColor;
        private LocalGoChessBasket m_chessBasket;
        private LocalMapObj m_map;

        public IEnumerator OnTurnToRobot()
        {
            #region �����㷨δʵ��ʱ����ʱ���
            //var xMax = m_map.Grids.Width;
            //var zMax = m_map.Grids.Height;
            //var x = Random.Range(0, xMax);
            //var z = Random.Range(0, zMax);
            //while (m_map.Grids[x, z].Occupied)
            //{
            //    x = Random.Range(0, xMax);
            //    z = Random.Range(0, zMax);
            //}
            //var piece = m_chessBasket.Get(m_robotColor);
            //m_map.Grids[x, z].AttachArea.Attach(piece);
            #endregion

            #region Ȩֵ��

            //m_maxX = 0;
            //m_maxY = 0;
            m_maxWeight = 0;
            m_caculateWeights.Clear();

            for (int i = 0; i < m_map.Grids.Width; i++)
            {
                yield return null;//ÿ֡����19�������µ�ÿ֡�ܼ������=>19*8*5 = 760��

                for (int j = 0; j < m_map.Grids.Height; j++)
                {
                    if (m_map.Grids[i, j].Occupied) continue;//�����㵱ǰ�����ӵ����������ӵ�����ֱ������

                    m_weights[i, j] = 0;
                    for (int k = 0; k < m_directions.Length; k++)
                    {
                        m_weights[i, j] += GetWeight(m_map.Grids[i, j], m_directions[k]);
                    }

                    if (m_maxWeight < m_weights[i, j])
                    {
                        m_caculateWeights.Clear();
                        m_maxWeight = m_weights[i, j];
                        m_caculateWeights.Add(new CaculateWeight(m_weights[i, j], i, j));
                    }
                    else if (m_maxWeight == m_weights[i, j])
                    {
                        m_caculateWeights.Add(new CaculateWeight(m_weights[i, j], i, j));
                    }
                }
            }

            ///����Ȩֵ���㼴ΪAI���ӵ㣬���ж�����Ȩֵ������������õİ취���������ڿ����������ĵĵط���
            CaculateWeight finalDropPoint = m_caculateWeights.GetRandomItem();
            var piece = m_chessBasket.Get(m_robotColor);
            m_map.Grids[finalDropPoint.i, finalDropPoint.j].AttachArea.Attach(piece);

            #endregion
        }


        /// <summary>
        /// ����Ŀ����ڸ��������ϵ�Ȩֵ
        /// </summary>
        /// <param name="gridData">��֤Ŀ�����δ���Ӽ���</param>
        /// <param name="direction">�˸�����</param>
        /// <returns></returns>
        private int GetWeight(LocalGridData gridData, Vector2Int direction)
        {
            string situation = "";
            Vector2Int pos = new Vector2Int(gridData.X, gridData.Z) + direction;

            //��ĳ��������������ö����
            int gridCnt = 0;
            int gridMax = 5;
            while (pos.x >= 0 && pos.x < m_map.Grids.Width && pos.y >= 0 && pos.y < m_map.Grids.Height
                && gridCnt < gridMax)
            {
                if (!m_map.Grids[pos.x, pos.y].Occupied) break;

                if (m_map.Grids[pos.x, pos.y].DragObject is LocalGoChessPiece piece)
                {
                    if(piece.VirtualColor == GoChessColor.Black)
                        situation += "1";
                    else if (piece.VirtualColor == GoChessColor.White)
                        situation += "2";
                }

                pos += direction;
                gridCnt++;
            }

            if (!m_weightHashMap.ContainsKey(situation))
            {
                return 0;
            }
            else
            {
                //Debug.Log("m_weightHashMap[situation:" + m_weightHashMap[situation]);
                return m_weightHashMap[situation];
            }
        }

        private int m_maxWeight = 0;
        private List<CaculateWeight> m_caculateWeights;
        public struct CaculateWeight
        {
            public int weight;
            public int i;
            public int j;

            public CaculateWeight(int w, int x, int y)
            {
                weight = w;
                i = x; 
                j = y;
            }
        }

        private EasyGrid<int> m_weights;
        private void InitWeights(int width, int height)
        {
            m_weights = new EasyGrid<int>(width, height);
            m_caculateWeights = new List<CaculateWeight>();
        }

        private Vector2Int[] m_directions = new Vector2Int[]
        {
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
        };

        private Dictionary<string, int> m_weightHashMap;
        private void InitValueMap()
        {
            m_weightHashMap = new Dictionary<string, int>();

            //Ȩֵ����Դ->https://blog.csdn.net/TheBug114514/article/details/115026320
            //��-0����-1����-2
            m_weightHashMap["1"] = 20;
            m_weightHashMap["11"] = 410;
            m_weightHashMap["111"] = 500;
            m_weightHashMap["1111"] = 8000;

            m_weightHashMap["2"] = 8;
            m_weightHashMap["22"] = 80;
            m_weightHashMap["222"] = 470;
            m_weightHashMap["2222"] = 9000;

            m_weightHashMap["12"] = 4;
            m_weightHashMap["112"] = 70;
            m_weightHashMap["1112"] = 450;
            m_weightHashMap["11112"] = 8000;

            m_weightHashMap["21"] = 6;
            m_weightHashMap["221"] = 60;
            m_weightHashMap["2221"] = 600;
            m_weightHashMap["22221"] = 10000;
        }


    }
}
