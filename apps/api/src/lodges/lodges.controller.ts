import {
  Body,
  Controller,
  Get,
  Param,
  ParseUUIDPipe,
  Post,
} from '@nestjs/common';
import { createLodgeValidationPipe } from './lodges.pipes.js';
import type { CreateLodgeDto } from './lodges.schema.js';
import { LodgesService } from './lodges.service.js';

@Controller('lodges')
export class LodgesController {
  constructor(private readonly lodgesService: LodgesService) {}

  @Get()
  findAll() {
    return this.lodgesService.findAll();
  }

  @Get(':id')
  findOne(@Param('id', ParseUUIDPipe) id: string) {
    return this.lodgesService.findOne(id);
  }

  @Post()
  create(@Body(createLodgeValidationPipe) createLodgeDto: CreateLodgeDto) {
    return this.lodgesService.create(createLodgeDto);
  }
}
