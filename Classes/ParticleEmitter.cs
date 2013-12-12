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
    class ParticleEmitter
    {
        public Vector2 position;

        int emitsPerSecond;

        List<Particle> particles;

        EParticleType type;


        public ParticleEmitter(Vector2 position, EParticleType type, int emitsPerSecond)
        {
            this.type = type;
            this.position = position;
            this.emitsPerSecond = emitsPerSecond;

            this.particles = new List<Particle>();
        }

        public void update(GameTime gameTime)
        {
            //TODO: currently its tick based, make it timebased:
            emit(type, gameTime);
            emit(type, gameTime);
            emit(type, gameTime);


            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles.ElementAt(i);

                p.update(gameTime);

                if (p.lifeTime <= 0)
                    particles.Remove(p);

            }
        }

        public void draw(SpriteBatch sb)
        {
            foreach (Particle p in particles)
                p.draw(sb);
        }

        private void emit(EParticleType type, GameTime gametime)
        {
            switch (type)
            {
                case EParticleType.torchParticle:


                    Vector2 target = new Vector2(position.X, position.Y - 128);
             
                    Vector2 positionOffset = new Vector2(position.X + (float)Math.Sin(Math.PI*4*Kaibo_Crawler.random.NextDouble()) * 32.0f, position.Y);
                    Vector2 direction = target - positionOffset;
                    direction.Normalize();
           

                    float speed = 1;
                    double lifeTime = 2.0d;


                    particles.Add(new Particle(Kaibo_Crawler.particle, positionOffset, direction, speed, lifeTime));


                    break;
            }
        }

    }
}
