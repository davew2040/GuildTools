import {
  Directive,
  Input,
  Output,
  EventEmitter,
  HostBinding,
  HostListener
} from '@angular/core';

@Directive({ selector: '[long-touch]' })
export class LongTouchDirective {

  @Input() duration = 500;

  @Output() onLongTouch: EventEmitter<any> = new EventEmitter();
  @Output() onLongTouching: EventEmitter<any> = new EventEmitter();
  @Output() onLongTouchEnd: EventEmitter<any> = new EventEmitter();

  private pressing: boolean;
  private longPressing: boolean;
  private timeout: any;
  private touchX = 0;
  private touchY = 0;

  @HostBinding('class.press')
  get press() { return this.pressing; }

  @HostBinding('class.longpress')
  get longPress() { return this.longPressing; }

  @HostListener('touchstart', ['$event'])
  onMouseDown(event) {
    this.touchX = event.touches[0].clientX;
    this.touchY = event.touches[0].clientY;

    this.pressing = true;
    this.longPressing = false;

    this.timeout = setTimeout(() => {
      this.longPressing = true;
      this.onLongTouch.emit(event);
      this.loop(event);
    }, this.duration);

    this.loop(event);
  }

  @HostListener('touchmove', ['$event'])
  onMouseMove(event) {
    if(this.pressing && !this.longPressing) {
      const xThres = (event.touches[0].clientX - this.touchX) > 10;
      const yThres = (event.touches[0].clientY - this.touchY) > 10;
      if(xThres || yThres) {
        this.endPress();
      }
    }
  }

  loop(event) {
    if(this.longPressing) {
      this.timeout = setTimeout(() => {
        this.onLongTouching.emit(event);
        this.loop(event);
      }, 50);
    }
  }

  endPress() {
    clearTimeout(this.timeout);
    this.longPressing = false;
    this.pressing = false;
    this.onLongTouchEnd.emit(true);
  }

  @HostListener('touchend')
  onMouseUp() { this.endPress(); }
}
