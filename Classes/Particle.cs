using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kaibo_Crawler
{
    public enum EParticleType {torchParticle, };

    class Particle
    {
        public static Rectangle particleSource = new Rectangle(0,0,256,256);
        public static Vector2 origin = new Vector2(128);


        Color color;

        Texture2D texture;

        Vector2 position;
        Vector2 direction;

        float speed;
        float rotation;
        float scale;

        public double lifeTime;

        double maxLifetime;

        float rotateDir = -1;

        public Particle(Texture2D texture, Vector2 position, Vector2 direction, float speed, double lifeTime)
        {
            this.texture = texture;
            this.position = position;
            this.direction = direction;
            this.speed = speed;
            this.lifeTime = lifeTime;

            this.scale = (float)(0.5*Kaibo_Crawler.random.NextDouble());
            this.rotation = (float)(Math.PI* Kaibo_Crawler.random.NextDouble());

            if (Kaibo_Crawler.random.NextDouble() < 0.5)
                rotateDir *= -1;

            maxLifetime = lifeTime;
        }

        public void update(GameTime gameTime)
        {
            this.position = this.position + this.direction * this.speed;
            lifeTime -= gameTime.ElapsedGameTime.TotalSeconds;

            rotation += rotateDir * 0.005f;
           

            float t = (float)(lifeTime / maxLifetime);

            color = Color.Lerp(Color.Red, Color.Yellow, t);
        }

        public void draw(SpriteBatch sb)
        {
            sb.Draw(texture, position, particleSource, color * (float)Math.Min(1.0d, lifeTime), rotation, origin, scale, SpriteEffects.None, 0);
        }

    }
}
