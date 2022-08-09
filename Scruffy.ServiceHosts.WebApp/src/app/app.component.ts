import { Component } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { authConfig } from './authConfig';

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

    oauthService.configure(authConfig);
    oauthService.loadDiscoveryDocumentAndTryLogin();
    oauthService.setupAutomaticSilentRefresh();
  }

  login() {
    if (this.oauthService.hasValidAccessToken() == false) {
      this.oauthService.initLoginFlow();
    }

    this.isLoggedIn = true;
  }

  logout() {
    this.oauthService.logOut();
    this.isLoggedIn = false;
  }
}
