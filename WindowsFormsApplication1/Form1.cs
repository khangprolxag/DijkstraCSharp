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
        Graphics graphics; //Thư viện vẽ của C#
        List<Node> graphNodes = new List<Node>(); //Danh sách quản lí các nút (quản lí vị trí, hàm vẽ nút ,...)

        bool stopCreateNode = false;  //Biến boolean kiểm tra người dùng đã nhấn "finish creating node"
        bool settingStartNode = false;  //Biến boolean kiểm tra người dùng đã nhấn "SET START Point"
        bool settingEndNode = false; //Biến boolean kiểm tra nút nhấn "SET END Point"
    

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void onClickOnForm(object sender, EventArgs e) //Sự kiện được gọi sau mỗi lần click chuột lên 
        //window form, mình dùng hàm này để tạo các nút và cài đặt nút khởi đầu và nút kết thúc
        {
            MouseEventArgs mouseEvent = (e as MouseEventArgs);
            Point mousePosition = new Point(mouseEvent.X, mouseEvent.Y); //Lấy vị trí chuột
            
            if (stopCreateNode) //nếu người dùng chưa nhấn finish creating node
             //thì mình tiếp tục tạo các nút mới
            {
                //còn nếu đã nhấn rồi thì mình chỉ cho người dùng chọn nút bắt đầu và nút kết thúc
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
            else //Tạo nút mới
            {
                Node newNode = new Node(mousePosition, Color.Blue); //Tạo nút mới tại vị trí chuột
                graphNodes.Add(newNode); //thêm nút mới vào danh sách

                drawNodes(false); //xóa màn hình các nút và vẽ lại các nút đó, false nghĩa là không vẽ
                //các đường liên kết các nút
            }


            
        }

        void outputLog(string text) //hàm in ra console
        {
            debugLog.AppendText("\r\n" + text);
            debugLog.ScrollToCaret();
        }


        List<Node> selectedNode = new List<Node>(); //quản lí 2 nút đã nhấn bằng cách double click 
        private void onDoubleClickOnForm(object sender, EventArgs e)// hàm này được gọi
            //khi người dùng nhấn 2 lần "double click" lên form
            //mình xử dụng hàm này để cho người dùng chọn 2 nút, và code sẽ kết nối 2 nút đó
        {
            if (!stopCreateNode || graphNodes.Count == 0) //If user hasn't been finished creating nodes, we won't allow to select special node
            {
                return;
            }
            MouseEventArgs mouseEvent = e as MouseEventArgs;
            Node closestNode = Node.closestNodeTo(ref graphNodes,new Point(mouseEvent.X, mouseEvent.Y));
            //Tìm nút gần nhất đối với vị trí chuột sau khi double click

            //Console.WriteLine(closestNode.ID);

            
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; 
            //SmoothingMode nghĩa là hàm vẽ sẽ vẽ đẹp hơn là không gọi smoothingMode
            closestNode.Select(graphics);

            if (selectedNode.Count < 1) //nếu chưa chọn nút nào
            {
                selectedNode.Add(closestNode); //thêm nút vừa chọn vào
            }else //khi đã chọn 2 nút
            {
                selectedNode.Add(closestNode); //thêm nút còn lại vào danh sách đã chọn
                
                Node.connectNodes(graphics, ref weightBetween, selectedNode[0], selectedNode[1]); //kết nối
                //2 nút đã chọn

                outputLog("Đã kết nối hai nút: " + selectedNode[0].ID + " " + selectedNode[1].ID);

                drawNodes(true); //Xóa và vẽ lại các đường liên kết + các nút

                selectedNode.Clear(); //xóa danh sách đã chọn
                showSimulatedDataTable(); //hiển thị table trọng điểm

            }
        }


        private void drawNodes(bool drawLine) //Hàm vẽ nút + đường, nhận 1 boolean drawLine
           // khi true - thì hàm này sẽ vẽ các đường liên kết, false thì không
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

        private void dataView_CellValueChanged(object sender, DataGridViewCellEventArgs e)//Hàm sự kiện
            //được gọi khi giá trị trong bảng có thay đổi
        {
            double editedValue = Convert.ToDouble((sender as DataGridView).CurrentCell.Value);//cast giá trị
            //từ bảng thành double 

            weightBetween[e.ColumnIndex - 1, e.RowIndex] = editedValue; //Thay đổi giá trị trong ma trận trọng điểm
            weightBetween[e.RowIndex, e.ColumnIndex - 1] = editedValue; //Thay đổi giá trị trong ma trận trọng điểm


            drawNodes(true); //xóa và vẽ lại các liên kết (sau khi update trọng điểm)

            outputLog("Đã chỉnh sửa thành công");
            showSimulatedDataTable(); //xóa table và hiển thị lại table
        }

        void showSimulatedDataTable() //hiển thị table
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


        private void startDijkstra(object sender, EventArgs e) //Khởi động thuật toán dijkstra
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

            graphNodes[startNode].HighLight(graphics, Color.Yellow); //In màu vàng cho nút bắt đầu
            graphNodes[endNode].HighLight(graphics, Color.Red); //In màu đỏ cho nút kết thúc
        }

        private void printPath(int start, int finish, int[] path) //In ra đường đi ngắn nhất
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
