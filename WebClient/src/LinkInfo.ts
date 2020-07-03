import { createPopper } from '@popperjs/core';

export default class LinkInfo {
    private openedLink: HTMLAnchorElement = null;
    private popperInstance: any = null;
    private tooltip: HTMLDivElement = null;

    public initialize() {
        document.querySelector("body").addEventListener('mousemove', (e) => {
            var anchor = (<HTMLElement>e.target).closest('a');
            this.changeLink(<HTMLAnchorElement>anchor);
          }, false);

        this.tooltip = document.createElement("div");
        this.tooltip.setAttribute("class", "websitecacher-link");
        this.tooltip.style.display = "none";
        let text = document.createTextNode("loading...");
        let arrow = document.createElement("div");
        arrow.setAttribute("data-popper-arrow", "1");
        arrow.setAttribute("class", "arrow");
        this.tooltip.appendChild(text);
        this.tooltip.appendChild(arrow);

        document.body.appendChild(this.tooltip);
    }

    private changeLink(link: HTMLAnchorElement) {
        if (link !== this.openedLink) {
            this.close();
            if (link) {
                this.open(link);
            }
        }
        this.openedLink = link;
    }

    private async open(link: HTMLAnchorElement) {
        this.tooltip.style.display = "block";
        this.tooltip.innerHTML = "loading...";

        this.popperInstance = createPopper(link, this.tooltip, {
            modifiers: [
              {
                name: 'offset',
                options: {
                  offset: [0, 8],
                },
              },
            ],
          });

        let response = await fetch("/website-cacher://resource-status/" + link.getAttribute("data-websitecacher-link"));
        let data = await response.json();

        if (link == this.openedLink) {
            if (data.status == "not-found") {
                this.tooltip.innerHTML = "<div class='websitecacher-danger'>This URL is not cached at all</div>";
            }

            if (data.status == "not-downloaded") {
                this.tooltip.innerHTML = "<div class='websitecacher-warning'>This URL is not cached yet</div>";
            }

            if (data.status == "downloaded") {
                this.tooltip.innerHTML = "<div class='websitecacher-success'>This URL is cached</div><div>Date: <b>" + data.time + "</b></div>";
            }
        }
    }

    private close() {
        if (this.popperInstance) {
            this.popperInstance.destroy();
            this.popperInstance = null;
            this.tooltip.style.display = "none";
          }
    }
}