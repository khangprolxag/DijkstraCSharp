using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            graphics = CreateGraphics();
        }

        //================ DIJKSTRA DATA
        double[,] weightBetween = new double[100, 100];
        int startNode = -1 , endNode = -1;
        //=========================================
        Graphics graphics;
        List<Node> graphNodes = new List<Node>();

        bool stopCreateNode = false; //True if we don't want to create node anymore
        bool settingStartNode = false;
        bool settingEndNode = false;
        private Color randomColor() //Random color using RGB
        {
            Random random = new Random();
            return Color.FromArgb((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
        }

    

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Created graphics");
        }

        private void onClickOnForm(object sender, EventArgs e)
        {
            MouseEventArgs mouseEvent = (e as MouseEventArgs);
            Point mousePosition = new Point(mouseEvent.X, mouseEvent.Y);
            
            if (stopCreateNode)
            {
                if (settingStartNode)
                {
                    
                    settingStartNode = false;
                    Node closestNode = Node.closestNodeTo(ref graphNodes, mousePosition);
                    startNode = closestNode.ID;

                    outputLog("Đã chọn được nút khởi đầu: " + startNode);

                    graphNodes[startNode].HighLight(graphics, Color.Yellow);
                    
                }
                else if (settingEndNode)
                {
                    settingEndNode = false;
                    Node closestNode = Node.closestNodeTo(ref graphNodes, mousePosition);
                    endNode = closestNode.ID;
                    graphNodes[endNode].HighLight(graphics, Color.Red);

                    outputLog("Đã chọn được nút kết thúc: " + endNode);
                }
                return;
            } 
            else
            {
                Node newNode = new Node(mousePosition, Color.Blue);
                graphNodes.Add(newNode);

                graphics.Clear(Color.White);
                drawNodes(false);
            }


            
        }

        void outputLog(string text)
        {
            debugLog.AppendText("\r\n" + text);
            debugLog.ScrollToCaret();
        }


        List<Node> selectedNode = new List<Node>();
        private void onDoubleClickOnForm(object sender, EventArgs e)
        {
            if (!stopCreateNode || graphNodes.Count == 0) //If user hasn't been finished creating nodes, we won't allow to select special node
            {
                return;
            }
            MouseEventArgs mouseEvent = e as MouseEventArgs;
            Node closestNode = Node.closestNodeTo(ref graphNodes,new Point(mouseEvent.X, mouseEvent.Y));

            //Console.WriteLine(closestNode.ID);

            
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            closestNode.Select(graphics);

            if (selectedNode.Count < 1)
            {
                selectedNode.Add(closestNode);
            }else
            {
                selectedNode.Add(closestNode);
                
                Node.connectNodes(graphics, ref weightBetween, selectedNode[0], selectedNode[1]);
                outputLog("Đã kết nối hai nút: " + selectedNode[0].ID + " " + selectedNode[1].ID);
                graphics.Clear(Color.White);
                drawNodes(true);

                Console.WriteLine(weightBetween[selectedNode[0].ID, selectedNode[1].ID]);
                selectedNode.Clear();

            }
        }


        private void drawNodes(bool drawLine)
        {

            graphics.Clear(Color.White);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (drawLine)
            {
                for (int i = 0; i < graphNodes.Count; i++)
                {
                    for(int j = 0; j < graphNodes.Count; j++)
                    {
                        if (weightBetween[i, j] != 0)
                        {

                            Node.drawLineBetween(graphics, Color.Black, weightBetween[i, j], graphNodes[i], graphNodes[j]);
                        }
                    }
                    
                }
            }
            

            foreach(Node node in graphNodes)
            {
                node.Draw(graphics);
            }

            
        }

        private void dataView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Console.WriteLine((sender as DataGridView).CurrentCell.Value.GetType());
            double editedValue = Convert.ToDouble((sender as DataGridView).CurrentCell.Value);

            weightBetween[e.ColumnIndex - 1, e.RowIndex] = editedValue;
            weightBetween[e.RowIndex, e.ColumnIndex - 1] = editedValue;

            
            drawNodes(true);

            outputLog("Đã chỉnh sửa thành công");
        }

        void showSimulatedDataTable()
        {

    
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("NODES");
            for(int i=0; i < graphNodes.Count; i++)
            { dataTable.Columns.Add("Node " + i); }

            for(int i=0; i < graphNodes.Count; i++)
            {
                //dataTable.Columns.Add("Node " + i);

                DataRow row;
                row = dataTable.NewRow();
                String rowName = "Node " + i;
                row["NODES"] = rowName;
                
                for(int j=0; j < graphNodes.Count; j++)
                {
                    row["Node " + j] = String.Format("{0:0.0}", weightBetween[i, j]);
                }

                dataTable.Rows.Add(row);

            }

            dataView.DataSource = dataTable;

            
            
        }

        private void finishCreatingNodes_Click(object sender, EventArgs e)
        {
            stopCreateNode = true;
            

        }

        private void SetStartPoint(object sender, EventArgs e)
        {
            showSimulatedDataTable();

            settingStartNode = true;

            (sender as Button).Enabled = false;
        }


        private void startDijkstra(object sender, EventArgs e)
        {
            
            
            int[] traceNode = new int[graphNodes.Count];
            double[] weightNode = new double[graphNodes.Count];
            int[] mark = new int[graphNodes.Count];
            int dijkstraStartNode = startNode;
            int dijkstraEndNode = endNode;
            //init ================
            outputLog("Khởi tạo giá trị mặc định cho dijkstra");
            for (int i=0; i < graphNodes.Count; i++)
            {
                traceNode[i] = -1;
                mark[i] = 0;
                weightNode[i] = double.MaxValue;
            }
            traceNode[dijkstraStartNode] = 0; //Start node to start node = 0 distance right?
            weightNode[dijkstraStartNode] = 0;
            //===============

            int connect;
            do
            {
                connect = -1;
                double minDistance = double.MaxValue;

                for (int ID = 0; ID < graphNodes.Count; ID++)
                {
                    if (mark[ID] == 0)
                    {
                        outputLog("nút " + ID + " Chưa đánh dấu... bắt đầu xét đỉnh đó");
                        if (weightBetween[dijkstraStartNode,ID] != 0 &&
    (weightNode[ID] > weightNode[dijkstraStartNode] + weightBetween[dijkstraStartNode, ID]))
                        {
                            outputLog("Cập nhật lại đường đi vì tìm thấy đường tốt hơn..." + dijkstraStartNode + " " + ID);
                            weightNode[ID] = weightNode[dijkstraStartNode] + weightBetween[dijkstraStartNode, ID];
                            traceNode[ID] = dijkstraStartNode;

                            Node.drawLineBetween(graphics, Color.Blue, graphNodes[ID], graphNodes[dijkstraStartNode]);
                        }

                        if (minDistance > weightNode[ID])
                        {
                            outputLog("Cập nhật lại trọng số bé nhất");
                            minDistance = weightNode[ID];
                            connect = ID;
                            //Node.drawLineBetween(graphics, Color.Green, graphNodes[connect], graphNodes[dijkstraStartNode]);

                        }
                    }
 
                }
                dijkstraStartNode = connect;
                mark[dijkstraStartNode] = 1;

                Thread.Sleep(1000);
            } while (connect != -1 && dijkstraStartNode != dijkstraEndNode);



            outputLog("TỔng trọng số đã đi qua là " + weightNode[endNode]);

            printPath(startNode, endNode, traceNode);
            graphNodes[startNode].HighLight(graphics, Color.Yellow);
            graphNodes[endNode].HighLight(graphics, Color.Red);
        }

        private void printPath(int start, int finish, int[] path)
        {
            int currentNode = finish;
            string tracePath = "";
            while (currentNode != start)
            {
                tracePath += currentNode + " <- ";
                Node.drawLineBetween(graphics, Color.Green, graphNodes[currentNode], graphNodes[path[currentNode]]);
                currentNode = path[currentNode];
            }
            Console.Write(start);
            tracePath += start;

            outputLog(tracePath);
        }



        private void SetEndPoint(object sender, EventArgs e)
        {
            settingEndNode = true;

            (sender as Button).Enabled = false;

            Console.WriteLine("Enable end node selecting");
        }
    }
}
