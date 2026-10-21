import {
  Body,
  Controller,
  Get,
  Param,
  ParseUUIDPipe,
  Post,
} from '@nestjs/common';
import {
  ApiBadRequestResponse,
  ApiBody,
  ApiCreatedResponse,
  ApiNotFoundResponse,
  ApiOkResponse,
} from '@nestjs/swagger';
import { schemaRef } from '../common/openapi.js';
import { createLodgeValidationPipe } from './lodges.pipes.js';
import {
  type CreateLodgeDto,
  createLodgeSchema,
  lodgeSchema,
} from './lodges.schema.js';
import { LodgesService } from './lodges.service.js';

@Controller('lodges')
export class LodgesController {
  constructor(private readonly lodgesService: LodgesService) {}

  @Get()
  @ApiOkResponse({ schema: { type: 'array', items: schemaRef(lodgeSchema) } })
  findAll() {
    return this.lodgesService.findAll();
  }

  @Get(':id')
  @ApiOkResponse({ schema: schemaRef(lodgeSchema) })
  @ApiBadRequestResponse({ description: 'Invalid lodge id (not a UUID)' })
  @ApiNotFoundResponse({ description: 'Lodge not found' })
  findOne(@Param('id', ParseUUIDPipe) id: string) {
    return this.lodgesService.findOne(id);
  }

  @Post()
  @ApiBody({ schema: schemaRef(createLodgeSchema) })
  @ApiCreatedResponse({ schema: schemaRef(lodgeSchema) })
  @ApiBadRequestResponse({ description: 'Validation failed' })
  create(@Body(createLodgeValidationPipe) createLodgeDto: CreateLodgeDto) {
    return this.lodgesService.create(createLodgeDto);
  }
}
