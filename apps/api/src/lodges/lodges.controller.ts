import { Controller, Get } from '@nestjs/common';
import { LodgesService } from './lodges.service.js';

@Controller('lodges')
export class LodgesController {
  constructor(private readonly lodgesService: LodgesService) {}

  @Get()
  findAll() {
    return this.lodgesService.findAll();
  }
}
