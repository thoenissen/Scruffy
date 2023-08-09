import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';
import { environment } from './app/environment';

const providers = [
  {
    provide: 'WEBAPI_BASE_URL',
    useFactory: () => environment.webApiBaseUrl,
    deps: [],
  },
];

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic(providers)
  .bootstrapModule(AppModule)
  .catch((err) => console.error(err));
