using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace Game3
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D texture;
        Texture2D inGraph;
        Texture2D inGraphTabl;
        Vector2 position;

        Color[] inGraphData;
        Color[] inGraphTablData;
        
        SpriteFont font;
        string currentCoordinats = "";
        string xString = "";
        string yString = "";

        double currentCoordinatsX;
        double currentCoordinatsY;

        double previousCoordinatsX;
        double previousCoordinatsY;

        // scale in pixels and origin
        double y0 = 426;
        double x0 = 25;
        double x1 = 74;
        double y1 = 50;

        SortedDictionary<double, double> soursData = new SortedDictionary<double, double>();
       
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //kursor
            texture = new Texture2D(this.GraphicsDevice, 10, 10);
            Color[] colorData = new Color[10 * 10];
            for (int i = 0; i < 100; i++)
                colorData[i] = Color.Purple;

            texture.SetData<Color>(colorData);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //
            inGraph = Content.Load<Texture2D>("line21");
            inGraphData = new Color[inGraph.Width * inGraph.Height];
            inGraph.GetData<Color>(inGraphData);

            inGraphTabl = Content.Load<Texture2D>("line22");
            inGraphTablData = new Color[inGraphTabl.Width * inGraphTabl.Height];
            inGraphTabl.GetData<Color>(inGraphTablData);
            //
            font = Content.Load<SpriteFont>("Tabl");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            //
            TouchCollection touchCollection = TouchPanel.GetState();
            if (touchCollection.Count > 0)
            {
                position.X = touchCollection[0].Position.X;
                position.Y = touchCollection[0].Position.Y;
            }
            //System.Diagnostics.Debug.WriteLine(position.X.ToString() + "," + position.Y.ToString());
            
            //
            if (position.X != previousCoordinatsX && position.Y != previousCoordinatsY)
            {
                int index = ((int)position.Y * inGraph.Width) + (int)position.X;

                if (index > 0 && index <= inGraphData.Length)
                    SetPoints();
                else
                    Collide();
            }
            previousCoordinatsX = position.X;
            previousCoordinatsY = position.Y;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();
            spriteBatch.Draw(inGraph, Vector2.Zero, Color.White);
            spriteBatch.Draw(inGraphTabl, new Vector2(0, inGraph.Height), Color.White);

            spriteBatch.Draw(texture, position, origin: new Vector2(5, 5));
            
            foreach (KeyValuePair<double, double> keyValue in soursData)
                spriteBatch.Draw(texture, new Vector2((int)(keyValue.Key * x1 + x0), (int)(y0 - keyValue.Value * y1)), rotation: -45f, origin: new Vector2(5, 5));

            if (String.Compare(currentCoordinats, "") != 0)
                    spriteBatch.DrawString(font, currentCoordinats, new Vector2(200, 605), Color.Black);

                spriteBatch.DrawString(font, xString, new Vector2(110, inGraph.Height+70), Color.Black);
                spriteBatch.DrawString(font, yString, new Vector2(110, inGraph.Height + 70+40), Color.Black);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected void Collide()
        {

            int varX = (int)position.X;
            int varY = (int)position.Y;

            int index = ((varY - inGraph.Height) * inGraphTabl.Width) + varX;

            if (index > 0 && index <= inGraphTablData.Length)
            {

                if (inGraphTablData[index] == Color.Lime)
                {
                    PaymentDrawLine();
                }
                else if (inGraphTablData[index] == Color.Yellow)
                {
                    AddPoints();
                }
            }
        }

        protected void SetPoints()
        {
            currentCoordinatsX = (position.X - x0) / x1;
            currentCoordinatsY = (y0 - position.Y) / y1;
            currentCoordinats = String.Format("{0:f}", currentCoordinatsX) + ", " + String.Format("{0:f}", currentCoordinatsY);
        }

        protected void AddPoints()
        {      
            try 
            {
                if (currentCoordinatsY != soursData[currentCoordinatsX])
                { 
                    //
                }
            }
            catch
            {
                soursData.Add(currentCoordinatsX, currentCoordinatsY);
                xString = xString + String.Format("{0:f}", currentCoordinatsX) + "             ";
                yString = yString + String.Format("{0:f}", currentCoordinatsY) + "             ";
            }
        }

        protected void PaymentDrawLine() {
            
            SortedDictionary<double, double> res = Polinom.MinSquearApproximation(soursData,0,0.07,6);

            SortedDictionary<double, double> lagrang = InterpolateLagrangePolynomial(soursData, 0, 0.09, 6);

            DrawLine(res, Color.Red);

            DrawLine(lagrang, Color.Brown);

        }

        protected void DrawLine(SortedDictionary<double, double> polinom, Color color)
        {

            foreach (KeyValuePair<double, double> keyValue in polinom)
            {
                
                int varX = (int)(keyValue.Key * x1 + x0);
                int varY = (int)(y0 - keyValue.Value * y1);

                for (int x = varX; x < varX + 5; x++)
                {
                    for (int y = varY; y < varY + 5; y++)
                    {

                        int index = (y * inGraph.Width) + x;

                        if (index > 0 && index <= inGraphData.Length)
                        {    
                            inGraphData[index] = color;
                            inGraph.SetData(inGraphData);
                        }
                        else
                        {
                            currentCoordinats = "Error! The value of the function is out of range.";
                            break;
                        }
                    }
                }
            }
        }


        public static SortedDictionary<double, double> InterpolateLagrangePolynomial(SortedDictionary<double, double> soursData, double start, double step, double end)
        {

            SortedDictionary<double, double> result = new SortedDictionary<double, double>();

            //For each x
            for (double i = start; i <= end; i += step)
            {
                double lagrangePol = 0;

                foreach (KeyValuePair<double, double> keyValue in soursData)
                {
                    double basicsPol = 1;
                    foreach (KeyValuePair<double, double> keyValueX in soursData)
                    {
                        if (keyValueX.Key != keyValue.Key)
                        {
                            basicsPol *= (i - keyValueX.Key) / (keyValue.Key - keyValueX.Key);
                        }
                    }
                    lagrangePol += basicsPol * keyValue.Value;
                }

                result.Add(i, lagrangePol);
            }
            return result;
        }
    }
}
