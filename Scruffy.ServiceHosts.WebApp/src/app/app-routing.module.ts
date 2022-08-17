import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RaidSetupComponent } from './raid/raid-setup/raid-setup.component';
import { UserRolesComponent } from './raid/user-roles/user-roles.component';

const routes: Routes = [
  { path: 'RaidSetup', component: RaidSetupComponent},
  { path: 'UserRoles', component: UserRolesComponent },
  { path: '**', component: LoginComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
