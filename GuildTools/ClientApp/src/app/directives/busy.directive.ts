import { Directive, ElementRef, Renderer2 } from '@angular/core';
import { BusyService } from '../shared-services/busy-service';

@Directive({
  selector: '[busyDirective]'
})
export class BusyDirective {

  constructor(el: ElementRef, private renderer: Renderer2, busyService: BusyService) {
    busyService.isBusySubscription.subscribe((isBusy) => {
      this.renderer.setStyle(el.nativeElement, 'display', isBusy ? 'inline': 'none');
    });
  }
}
