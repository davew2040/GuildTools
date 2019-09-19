import { Component, OnInit, ElementRef, Renderer2 } from '@angular/core';
import { BusyService } from 'app/shared-services/busy-service';
import { DataService } from 'app/services/data-services';

interface Point {
  readonly x: number;
  readonly y: number;
}

@Component({
  selector: 'app-sketch',
  templateUrl: './sketch.component.html',
  styleUrls: ['./sketch.component.css']
})
export class SketchComponent implements OnInit {

  private backgroundImageUri = 'assets/sketch/dais.jpg';
  private isDrawing = false;
  private movementQueue: Array<Point> = [];

  public get backgroundImageUriExpression() {
    return 'url(' + this.backgroundImageUri + ')';
  }

  public get pageLoaded() {
    return true;
  }

  constructor(
    public dataService: DataService,
    private busyService: BusyService
    ) {
      console.log(this.pageLoaded);
    }


  ngOnInit() {

  }

  public handleMouseDown(eventArgs: MouseEvent): void {
    this.isDrawing = true;

    const initialPoint = this.TransformEventCoordsToPoint(eventArgs);

    this.movementQueue = [].concat(initialPoint);
  }

  public handleMouseUp(): void {
    if (this.isDrawing){
      console.log("finish drawing");
    }

    this.isDrawing = false;
  }

  public handleMouseLeave(): void {
    if (this.isDrawing) {
      console.log("cancel drawing");
    }

    this.isDrawing = false;
  }

  public handleMove(eventArgs: MouseEvent): void {
    if (this.isDrawing) {
      const nextPoint = this.TransformEventCoordsToPoint(eventArgs);

      const canvas = <HTMLCanvasElement>(document.getElementById("drawing-canvas"));
      const ctx = canvas.getContext("2d");
      ctx.fillStyle = "#FF0000";
      ctx.strokeStyle = "#FF0000";

      const lastPoint = this.movementQueue[this.movementQueue.length - 1];
      ctx.beginPath();
        ctx.moveTo(lastPoint.x, lastPoint.y);
        ctx.lineTo(nextPoint.x, nextPoint.y);
        ctx.closePath();
      ctx.stroke();

      this.movementQueue.push(nextPoint);
    }
  }

  private TransformEventCoordsToPoint(eventArgs: MouseEvent) {
    const point: Point = {x: eventArgs.offsetX, y: eventArgs.offsetY };

    return point;
  }
}
