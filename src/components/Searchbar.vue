<template>
<div id='searchbar'>
  <b-input-group class="mt-3" id='search-bar'>
    <b-form-input id="search-input" v-model="text" @input="onSearch" placeholder="Search"></b-form-input>
    <b-input-group-append>
      <b-button class="b-button" @click="onReset" variant="info">&#x2715;</b-button>
    </b-input-group-append>
  </b-input-group>
</div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator'

import { BInputGroup, BFormInput, BSidebar, BInputGroupAppend } from 'bootstrap-vue'
Vue.component('b-input-group', BInputGroup)
Vue.component('b-input-group-append', BInputGroupAppend)
Vue.component('b-form-input', BFormInput)
Vue.component('b-sidebar', BSidebar)

@Component
export default class Searchbar extends Vue {

  text = ""

  // This method runs every time an input is given to the search bar
  // Emits the search text so other components can access it
  onSearch () {
    this.$emit('input', this.text)
  }
  
  // Removes text input and emits the click event to parent component (so Graph can use it to reset the graph)
  onReset () {
    this.text = ''
    this.$emit('button-click')
  }

  changeText(text: string){
    this.text = text
  }
  
}
</script>

<style scoped>
#searchbar {
  /* width: 100%; */
  width: 20em;
}
#search-bar {
  width: 20em;
  margin-top: 0 !important;
}
#search-input {
  background-color: var(--color-neutral-800);
  color: var(--color-neutral-100);
}
#search-input::placeholder {
  color: var(--color-neutral-300);
}
#search-input:focus {
  border-color: var(--color-primary-500);
  box-shadow: none;
}
</style>
