import { Component, OnInit } from '@angular/core';
import { ApiPipeService } from 'src/app/api-pipe.service';
import { appointment } from 'src/app/appointment';
import { participant } from 'src/app/participant';
import { lineUpParticipant } from 'src/app/lineUpParticipant';
import { lineUp } from 'src/app/lineUp';

@Component({
  selector: 'app-raid-setup',
  templateUrl: './raid-setup.component.html',
  styleUrls: ['./raid-setup.component.scss'],
})
export class RaidSetupComponent implements OnInit {
  currentAppointment?: appointment;

  healSelect1?: { group: string; user: participant };
  boonSelect1?: { group: string; user: participant };
  dpsSelect11?: { group: string; user: participant };
  dpsSelect12?: { group: string; user: participant };
  dpsSelect13?: { group: string; user: participant };
  healSelect2?: { group: string; user: participant };
  boonSelect2?: { group: string; user: participant };
  dpsSelect21?: { group: string; user: participant };
  dpsSelect22?: { group: string; user: participant };
  dpsSelect23?: { group: string; user: participant };

  disabledPlayers: number[] = [];

  dpsPlayers: participant[] = [];
  alacPlayers: participant[] = [];
  quickPlayers: participant[] = [];
  alacHealPlayers: participant[] = [];
  quickHealPlayers: participant[] = [];

  constructor(private apiPipe: ApiPipeService) {
    apiPipe.appointment$.subscribe((data) => {
      this.currentAppointment = data;
      for (let user of data.participants) {
        if (user.roles.includes(1))
          this.dpsPlayers.push(JSON.parse(JSON.stringify(user)));
        if (user.preferredRoles.includes(1)) {
          this.dpsPlayers[this.dpsPlayers.length - 1].name += '⭐';
        }
        if (user.roles.includes(2))
          this.alacPlayers.push(JSON.parse(JSON.stringify(user)));
        if (user.preferredRoles.includes(2)) {
          this.alacPlayers[this.alacPlayers.length - 1].name += '⭐';
        }
        if (user.roles.includes(3))
          this.quickPlayers.push(JSON.parse(JSON.stringify(user)));
        if (user.preferredRoles.includes(3)) {
          this.quickPlayers[this.quickPlayers.length - 1].name += '⭐';
        }
        if (user.roles.includes(4))
          this.alacHealPlayers.push(JSON.parse(JSON.stringify(user)));
        if (user.preferredRoles.includes(4)) {
          this.alacHealPlayers[this.alacHealPlayers.length - 1].name += '⭐';
        }
        if (user.roles.includes(5))
          this.quickHealPlayers.push(JSON.parse(JSON.stringify(user)));
        if (user.preferredRoles.includes(5)) {
          this.quickHealPlayers[this.quickHealPlayers.length - 1].name += '⭐';
        }
        if (user.roles.includes(6)) {
          this.alacPlayers.push(JSON.parse(JSON.stringify(user)));
          this.alacPlayers[this.alacPlayers.length - 1].name += '🛡️';
        }
        if (user.preferredRoles.includes(6)) {
          this.alacPlayers[this.alacPlayers.length - 1].name += '⭐';
        }
        if (user.roles.includes(7)) {
          this.quickPlayers.push(JSON.parse(JSON.stringify(user)));
          this.quickPlayers[this.quickPlayers.length - 1].name += '🛡️';
        }
        if (user.preferredRoles.includes(7)) {
          this.quickPlayers[this.quickPlayers.length - 1].name += '⭐';
        }
        if (user.roles.includes(8)) {
          this.alacHealPlayers.push(JSON.parse(JSON.stringify(user)));
          this.alacHealPlayers[this.alacHealPlayers.length - 1].name += '🛡️';
        }
        if (user.preferredRoles.includes(8)) {
          this.alacHealPlayers[this.alacHealPlayers.length - 1].name += '⭐';
        }
        if (user.roles.includes(9)) {
          this.quickHealPlayers.push(JSON.parse(JSON.stringify(user)));
          this.quickHealPlayers[this.quickHealPlayers.length - 1].name += '🛡️';
        }
        if (user.preferredRoles.includes(9)) {
          this.quickHealPlayers[this.quickHealPlayers.length - 1].name += '⭐';
        }
      }
    });
  }

  ngOnInit(): void {}
  onSelect(event: any) {
    this.getDisabledPlayers();
    console.log(event);
  }

  getDisabledPlayers() {
    this.disabledPlayers = [];
    if (this.healSelect1) this.disabledPlayers.push(this.healSelect1.user.id);
    if (this.boonSelect1) this.disabledPlayers.push(this.boonSelect1.user.id);
    if (this.dpsSelect11) this.disabledPlayers.push(this.dpsSelect11.user.id);
    if (this.dpsSelect12) this.disabledPlayers.push(this.dpsSelect12.user.id);
    if (this.dpsSelect13) this.disabledPlayers.push(this.dpsSelect13.user.id);

    if (this.healSelect2) this.disabledPlayers.push(this.healSelect2.user.id);
    if (this.boonSelect2) this.disabledPlayers.push(this.boonSelect2.user.id);
    if (this.dpsSelect21) this.disabledPlayers.push(this.dpsSelect21.user.id);
    if (this.dpsSelect22) this.disabledPlayers.push(this.dpsSelect22.user.id);
    if (this.dpsSelect23) this.disabledPlayers.push(this.dpsSelect23.user.id);
  }

  postLineUp() {
    let users: lineUpParticipant[] = [];

    if (this.healSelect1?.user != null) {
      users.push({
        userId: this.healSelect1.user.id,
        roleId: 1,
      });
    }

    if (this.boonSelect1?.user != null) {
      users.push({
        userId: this.boonSelect1.user.id,
        roleId: 1,
      });
    }

    if (this.dpsSelect11?.user != null) {
      users.push({
        userId: this.dpsSelect11.user.id,
        roleId: 1,
      });
    }

    if (this.dpsSelect12?.user != null) {
      users.push({
        userId: this.dpsSelect12.user.id,
        roleId: 1,
      });
    }
    if (this.dpsSelect13?.user != null) {
      users.push({
        userId: this.dpsSelect13.user.id,
        roleId: 1,
      });
    }

    if (this.healSelect2?.user != null) {
      users.push({
        userId: this.healSelect2.user.id,
        roleId: 1,
      });
    }

    if (this.boonSelect2?.user != null) {
      users.push({
        userId: this.boonSelect2.user.id,
        roleId: 1,
      });
    }

    if (this.dpsSelect21?.user != null) {
      users.push({
        userId: this.dpsSelect21.user.id,
        roleId: 1,
      });
    }

    if (this.dpsSelect22?.user != null) {
      users.push({
        userId: this.dpsSelect22.user.id,
        roleId: 1,
      });
    }

    if (this.dpsSelect23?.user != null) {
      users.push({
        userId: this.dpsSelect23.user.id,
        roleId: 1,
      });
    }

    let lineUp: lineUp = {
      appointmentId: this.currentAppointment?.id!,
      groups: {
        '1': users,
      },
    };

    this.apiPipe.postLineUp(lineUp);
  }
}
