from django.db import models

class Company(models.Model):
    name = models.CharField(max_length=50)
    description = models.CharField(max_length=500)
    created_on = models.DateField() 
