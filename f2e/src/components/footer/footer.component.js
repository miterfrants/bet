import {
    BaseComponent
} from '../../swim/base.component.js';

export class FooterComponent extends BaseComponent {
    constructor(elRoot, variable, args) {
        super(elRoot, variable, args);
        this.id = 'FooterComponent';
    }

    async render() {
        await super.render({
            ...this.variable
        });
    }
}