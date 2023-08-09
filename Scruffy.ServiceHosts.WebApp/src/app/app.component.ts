import { Component } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { authConfig } from './authConfig';
import { environment } from './environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'WebApp';
  isLoggedIn = false;

  constructor(private oauthService: OAuthService) {
    this.oauthService = oauthService;

    if (environment.production) {
      oauthService.configure(authConfig);
      oauthService.loadDiscoveryDocumentAndTryLogin();
      oauthService.setupAutomaticSilentRefresh();
    } else {
      this.isLoggedIn = true;
    }
  }

  login() {
    if (environment.production) {
      if (this.oauthService.hasValidAccessToken() == false) {
        this.oauthService.initLoginFlow();
      }
      this.isLoggedIn = true;
    }
  }

  logout() {
    if (environment.production) {
      this.oauthService.logOut();
      this.isLoggedIn = false;
    }
  }
}
