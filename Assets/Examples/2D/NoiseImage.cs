using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NoiseImage : MonoBehaviour
{
  public float strokeLength = 25;
  public float noiseScale = 0.005f;

  //var imgNames = ["portrait.jpg", "fleurs.jpg", "frise.png" , "oiso.png", "arli.jpg" , "sirene.jpg"];
  public Texture2D image;
  Color[] imagePixels;

  public int drawLength = 250;  // The amount of times we'll iterate through draw() until it stops.
  int frame;  // Variable to contain the current frame.


  void Start() {
    Draw.infiniteMode = true;
    Draw.EnableTransparentMode(true);
    Draw.SetValuesAsPixels(true);
    changeImage();
  }


  void Update() {
      // Stop drawing once we exceed the drawing length.
    if (frame > drawLength) {
      return;
    }

    //let img = imgs[imgIndex];
    
    Vector3 offset = new Vector3(
      Screen.width / 2 - image.width / 2,
      Screen.height / 2 - image.height / 2,
      0
    );
    
    // The smaller the stroke is the more the spawn count increases to capture more detail.
    // map n'existe pas dans unity, il faut faire en deux étapes
    int count = (int)map(frame, 0, drawLength, 2, 80);

    for (int i = 0; i < count; i++) {  // Add a new loop to create multiple strokes.
      int x = Random.Range(0, image.width);
      int y = Random.Range(0, image.height);

      int index = (y * image.width + x);

      Color c = imagePixels[index];
      Draw.color = c;

      // Map the thickness based on the current frame of the sketch.
       // First it starts off thick, then gradually thins out until it reaches zero.
      int strokeThickness = (int)map(frame, 0, drawLength, 15, 0);
      Draw.stroke = strokeThickness;


      float n = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
      Quaternion rotation = Quaternion.AngleAxis(map(n, 0, 1, -180, 180), Vector3.forward);
      float lengthVariation = Random.Range(0.75f, 1.25f);  // Randomize a multiplier to make length shorter or longer.
      Vector3 position = new Vector3(x, y,0);
      Draw.RoundedLine(offset + position, rotation, strokeLength * lengthVariation);
      //line(0, 0, strokeLength * lengthVariation, 0);
    }

    frame++;  // Increase frame by 1.
  }


  void changeImage() {
    frame = 0;
    imagePixels = image.GetPixels();
  }

  // map n'existe pas dans unity, on ajoute donc la fonction ici
  float map(float val, float sourcemin, float sourcemax, float destmin, float destmax)
  {
    float percent = Mathf.InverseLerp(sourcemin, sourcemax, val);
    return Mathf.Lerp(destmin, destmax, percent);
  }
}
