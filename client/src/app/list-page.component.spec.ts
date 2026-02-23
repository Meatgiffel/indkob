import { FormBuilder } from '@angular/forms';

import { ListPageComponent } from './list-page.component';

describe('ListPageComponent (logic)', () => {
  function createComponent(): ListPageComponent {
    const fb = new FormBuilder();
    const api = {} as any;
    const toast = { add: () => undefined } as any;
    const realtime = {} as any;
    return new ListPageComponent(fb, api, toast, realtime);
  }

  it('uses raw typed name when selecting "Opret ..." option', async () => {
    const component = createComponent();
    const createSpy = spyOn(component, 'createItemFromSearch').and.resolveTo();

    component.selectItem({
      value: {
        id: -1,
        name: 'Opret "mælk"',
        area: 'Mejeri',
        _create: true,
        _createName: 'mælk'
      }
    });

    expect(createSpy).toHaveBeenCalledWith('mælk');
  });

  it('creates from Enter when no item is selected', () => {
    const component = createComponent();
    component.searchTerm = 'bananer';
    component.typedTerm = 'bananer';
    component.entryForm.patchValue({ itemId: null });
    const createSpy = spyOn(component, 'createItemFromSearch').and.resolveTo();
    const preventDefault = jasmine.createSpy('preventDefault');

    component.maybeCreateFromEnter({ key: 'Enter', preventDefault } as any);

    expect(preventDefault).toHaveBeenCalled();
    expect(createSpy).toHaveBeenCalled();
  });
});
